using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Companies;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Security;

namespace Nexus.Application.UseCases.Companies;

public class CompanyService(
    ICompanyRepository repository,
    IValidator<CreateCompanyDto> createValidator,
    IValidator<UpdateCompanyDto> updateValidator,
    IValidator<CompanySearchRequest> searchValidator,
    ILogger<CompanyService> logger) : ICompanyService
{
    public async Task<Response<CompanyDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var company = await repository.GetByIdAsync(id, ct);

        return company is null
            ? Response<CompanyDto>.Fail("Company not found", ErrorCode.NotFound)
            : Response<CompanyDto>.Ok(MapToDto(company));
    }

    public async Task<Response<IReadOnlyList<CompanyDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var companies = await repository.GetAllAsync(ct);
        return Response<IReadOnlyList<CompanyDto>>.Ok(companies.Select(MapToDto).ToList());
    }

    public async Task<ResponsePagination<CompanyDto>> SearchAsync(CompanySearchRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await searchValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponsePagination<CompanyDto>();
        }

        var (items, totalCount) = await repository.SearchAsync(
            request.SearchTerm,
            request.Page,
            request.PageSize,
            ct);

        return ResponsePagination<CompanyDto>.Ok(
            items.Select(MapToDto).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    public async Task<Response<CompanyDto>> CreateAsync(CreateCompanyDto dto, CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<CompanyDto>();
        }

        if (await repository.ExistsByTaxIdAsync(dto.TaxId, ct))
        {
            logger.LogWarning("Create company failed: TaxId already exists [{TaxId}]", dto.TaxId);
            return Response<CompanyDto>.Fail("A company with this TaxId already exists", ErrorCode.Conflict);
        }

        var company = new Company { Name = dto.Name, TaxId = dto.TaxId, IsActive = true };
        var created = await repository.AddAsync(company, ct);

        logger.LogInformation("Company created [{CompanyId}] [{Name}] [{TaxId}]", created.Id, created.Name,
            created.TaxId);

        return Response<CompanyDto>.Ok(MapToDto(created), "Company created successfully");
    }

    public async Task<Response<CompanyDto>> UpdateAsync(long id, UpdateCompanyDto dto,
        CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<CompanyDto>();
        }

        var company = await repository.GetByIdAsync(id, ct);
        if (company is null)
        {
            logger.LogWarning("Update company failed: company not found [{CompanyId}]", id);
            return Response<CompanyDto>.Fail("Company not found", ErrorCode.NotFound);
        }

        company.Name = dto.Name;
        company.IsActive = dto.IsActive;

        await repository.UpdateAsync(company, ct);

        logger.LogInformation("Company updated [{CompanyId}] [{Name}]", id, company.Name);

        return Response<CompanyDto>.Ok(MapToDto(company), "Company updated successfully");
    }

    public async Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var company = await repository.GetByIdAsync(id, ct);
        if (company is null)
        {
            logger.LogWarning("Delete company failed: company not found [{CompanyId}]", id);
            return Response<bool>.Fail("Company not found", ErrorCode.NotFound);
        }

        await repository.DeleteAsync(id, ct);

        logger.LogInformation("Company deleted (soft-delete) [{CompanyId}] [{Name}]", id, company.Name);

        return Response<bool>.Ok(true, "Company deleted successfully");
    }

    private static CompanyDto MapToDto(Company company)
    {
        return new CompanyDto(
            company.Id,
            company.Name,
            company.TaxId,
            company.IsActive,
            company.CreatedAt,
            company.UpdatedAt);
    }
}
