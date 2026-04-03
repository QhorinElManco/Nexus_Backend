using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Suppliers;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Products;

namespace Nexus.Application.UseCases.Suppliers;

public class SupplierService(
    ISupplierRepository repository,
    IValidator<CreateSupplierDto> createValidator,
    IValidator<UpdateSupplierDto> updateValidator,
    IValidator<SupplierSearchRequest> searchValidator,
    ILogger<SupplierService> logger) : ISupplierService
{
    public async Task<Response<SupplierDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var supplier = await repository.GetByIdAsync(id, ct);

        if (supplier is not null && supplier.CompanyId == companyId)
        {
            return Response<SupplierDto>.Ok(MapToDto(supplier));
        }

        logger.LogWarning("Get supplier failed: supplier not found [{SupplierId}] [{CompanyId}]", id, companyId);
        return Response<SupplierDto>.Fail("Supplier not found", ErrorCode.NotFound);
    }

    public async Task<Response<IReadOnlyList<SupplierDto>>> GetAllAsync(long companyId, CancellationToken ct = default)
    {
        var suppliers = await repository.GetAllAsync(ct);
        var filtered = suppliers.Where(s => s.CompanyId == companyId).ToList();
        return Response<IReadOnlyList<SupplierDto>>.Ok(filtered.Select(MapToDto).ToList());
    }

    public async Task<ResponsePagination<SupplierDto>> SearchAsync(SupplierSearchRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await searchValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponsePagination<SupplierDto>();
        }

        var (items, totalCount) = await repository.SearchAsync(
            request.CompanyId,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            ct);

        return ResponsePagination<SupplierDto>.Ok(
            items.Select(MapToDto).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    public async Task<Response<SupplierDto>> CreateAsync(long companyId, CreateSupplierDto dto,
        CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<SupplierDto>();
        }

        if (await repository.ExistsByTaxIdAsync(companyId, dto.TaxId, ct))
        {
            logger.LogWarning("Create supplier failed: TaxId already exists [{CompanyId}] [{TaxId}]", companyId,
                dto.TaxId);
            return Response<SupplierDto>.Fail("A supplier with this TaxId already exists", ErrorCode.Conflict);
        }

        var supplier = new Supplier { CompanyId = companyId, Name = dto.Name, TaxId = dto.TaxId };
        var created = await repository.AddAsync(supplier, ct);

        logger.LogInformation("Supplier created [{SupplierId}] [{CompanyId}] [{Name}] [{TaxId}]",
            created.Id, created.CompanyId, created.Name, created.TaxId);

        return Response<SupplierDto>.Ok(MapToDto(created), "Supplier created successfully");
    }

    public async Task<Response<SupplierDto>> UpdateAsync(long id, UpdateSupplierDto dto, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<SupplierDto>();
        }

        var supplier = await repository.GetByIdAsync(id, ct);
        if (supplier is null || supplier.CompanyId != companyId)
        {
            logger.LogWarning("Update supplier failed: supplier not found [{SupplierId}] [{CompanyId}]", id, companyId);
            return Response<SupplierDto>.Fail("Supplier not found", ErrorCode.NotFound);
        }

        supplier.Name = dto.Name;

        await repository.UpdateAsync(supplier, ct);

        logger.LogInformation("Supplier updated [{SupplierId}] [{CompanyId}] [{Name}]", id, companyId, supplier.Name);

        return Response<SupplierDto>.Ok(MapToDto(supplier), "Supplier updated successfully");
    }

    public async Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default)
    {
        var supplier = await repository.GetByIdAsync(id, ct);
        if (supplier is null || supplier.CompanyId != companyId)
        {
            logger.LogWarning("Delete supplier failed: supplier not found [{SupplierId}] [{CompanyId}]", id, companyId);
            return Response<bool>.Fail("Supplier not found", ErrorCode.NotFound);
        }

        await repository.DeleteAsync(id, ct);

        logger.LogInformation("Supplier deleted (soft-delete) [{SupplierId}] [{CompanyId}] [{Name}]",
            id, companyId, supplier.Name);

        return Response<bool>.Ok(true, "Supplier deleted successfully");
    }

    private static SupplierDto MapToDto(Supplier supplier)
    {
        return new SupplierDto(
            supplier.Id,
            supplier.CompanyId,
            supplier.Name,
            supplier.TaxId,
            supplier.CreatedAt,
            supplier.UpdatedAt);
    }
}
