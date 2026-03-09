using Nexos.Application.Dto.Companies;
using Nexos.Application.Interfaces.Repositories;
using Nexos.Domain.Entity.Security;
using Nexos.Transversal.Common;
using Nexos.Transversal.Common.Response;

namespace Nexos.Application.UseCases.Companies;

public interface ICompanyService
{
    public Task<Response<CompanyDto>> GetByIdAsync(long id, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<CompanyDto>>> GetAllAsync(CancellationToken ct = default);

    public Task<ResponsePagination<CompanyDto>> SearchAsync(CompanySearchRequest request,
        CancellationToken ct = default);

    public Task<Response<CompanyDto>> CreateAsync(CreateCompanyDto dto, CancellationToken ct = default);
    public Task<Response<CompanyDto>> UpdateAsync(long id, UpdateCompanyDto dto, CancellationToken ct = default);
    public Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
}

public class CompanyService(ICompanyRepository repository) : ICompanyService
{
    public async Task<Response<CompanyDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var company = await repository.GetByIdAsync(id, ct);
        if (company is null)
        {
            return Response<CompanyDto>.Fail("Company not found", ErrorCode.NotFound);
        }

        return Response<CompanyDto>.Ok(MapToDto(company));
    }

    public async Task<Response<IReadOnlyList<CompanyDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var companies = await repository.GetAllAsync(ct);
        return Response<IReadOnlyList<CompanyDto>>.Ok(companies.Select(MapToDto).ToList());
    }

    public async Task<ResponsePagination<CompanyDto>> SearchAsync(CompanySearchRequest request,
        CancellationToken ct = default)
    {
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
        if (await repository.ExistsByTaxIdAsync(dto.TaxId, ct))
        {
            return Response<CompanyDto>.Fail("A company with this TaxId already exists", ErrorCode.Conflict);
        }

        var company = new Company { Name = dto.Name, TaxId = dto.TaxId, IsActive = true };
        var created = await repository.AddAsync(company, ct);
        return Response<CompanyDto>.Ok(MapToDto(created), "Company created successfully");
    }

    public async Task<Response<CompanyDto>> UpdateAsync(long id, UpdateCompanyDto dto, CancellationToken ct = default)
    {
        var company = await repository.GetByIdAsync(id, ct);
        if (company is null)
        {
            return Response<CompanyDto>.Fail("Company not found", ErrorCode.NotFound);
        }

        company.Name = dto.Name;
        company.IsActive = dto.IsActive;

        await repository.UpdateAsync(company, ct);
        return Response<CompanyDto>.Ok(MapToDto(company), "Company updated successfully");
    }

    public async Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var company = await repository.GetByIdAsync(id, ct);
        if (company is null)
        {
            return Response<bool>.Fail("Company not found", ErrorCode.NotFound);
        }

        await repository.DeleteAsync(id, ct);
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
