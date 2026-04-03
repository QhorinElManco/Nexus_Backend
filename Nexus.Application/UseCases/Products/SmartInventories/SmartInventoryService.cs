using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Products;

namespace Nexus.Application.UseCases.Products.SmartInventories;

public class SmartInventoryService(
    ISmartInventoryRepository repository,
    IWarehouseRepository warehouseRepository,
    ISupplierRepository supplierRepository,
    ISkuRepository skuRepository,
    IValidator<CreateSmartInventoryDto> createValidator,
    IValidator<UpdateSmartInventoryDto> updateValidator,
    ILogger<SmartInventoryService> logger) : ISmartInventoryService
{
    public async Task<Response<SmartInventoryDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var smartInventory = await repository.GetByIdAsync(id, ct);

        if (smartInventory is not null && smartInventory.Warehouse.CompanyId == companyId)
        {
            return Response<SmartInventoryDto>.Ok(MapToDto(smartInventory));
        }

        logger.LogWarning("Get SmartInventory failed: not found [{SmartInventoryId}] [{CompanyId}]", id, companyId);
        return Response<SmartInventoryDto>.Fail("SmartInventory not found", ErrorCode.NotFound);

    }

    public async Task<Response<IReadOnlyList<SmartInventoryDto>>> GetAllAsync(long companyId,
        CancellationToken ct = default)
    {
        var smartInventories = await repository.GetAllByCompanyAsync(companyId, ct);
        return Response<IReadOnlyList<SmartInventoryDto>>.Ok(
            smartInventories.Select(MapToDto).ToList());
    }

    public async Task<ResponsePagination<SmartInventoryDto>> SearchAsync(SmartInventorySearchRequest request,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await repository.SearchAsync(
            request.CompanyId,
            request.SearchTerm,
            request.WarehouseId,
            request.SkuId,
            request.SupplierId,
            request.Page,
            request.PageSize,
            ct);

        return ResponsePagination<SmartInventoryDto>.Ok(
            items.Select(MapToDto).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    public async Task<Response<SmartInventoryDto>> CreateAsync(CreateSmartInventoryDto dto,
        CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<SmartInventoryDto>();
        }

        var warehouse = await warehouseRepository.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            logger.LogWarning("Create SmartInventory failed: warehouse not found [{WarehouseId}]", dto.WarehouseId);
            return Response<SmartInventoryDto>.Fail("Warehouse not found", ErrorCode.NotFound);
        }

        var sku = await skuRepository.GetByIdAsync(dto.SkuId, ct);
        if (sku is null)
        {
            logger.LogWarning("Create SmartInventory failed: SKU not found [{SkuId}]", dto.SkuId);
            return Response<SmartInventoryDto>.Fail("SKU not found", ErrorCode.NotFound);
        }

        var supplier = await supplierRepository.GetByIdAsync(dto.SupplierId, ct);
        if (supplier is null)
        {
            logger.LogWarning("Create SmartInventory failed: supplier not found [{SupplierId}]", dto.SupplierId);
            return Response<SmartInventoryDto>.Fail("Supplier not found", ErrorCode.NotFound);
        }

        // Validate Warehouse and Supplier belong to same company
        if (warehouse.CompanyId != supplier.CompanyId)
        {
            logger.LogWarning(
                "Create SmartInventory failed: Warehouse and Supplier must belong to same company [{WarehouseId}] [{SupplierId}]",
                dto.WarehouseId, dto.SupplierId);
            return Response<SmartInventoryDto>.Fail(
                "Warehouse and Supplier must belong to same company", ErrorCode.BusinessRule);
        }

        // Check for duplicate WarehouseId + SkuId
        if (await repository.ExistsByWarehouseAndSkuAsync(dto.WarehouseId, dto.SkuId, null, ct))
        {
            logger.LogWarning(
                "Create SmartInventory failed: SmartInventory already exists for this Warehouse and SKU [{WarehouseId}] [{SkuId}]",
                dto.WarehouseId, dto.SkuId);
            return Response<SmartInventoryDto>.Fail(
                "SmartInventory already exists for this Warehouse and SKU", ErrorCode.Conflict);
        }

        var smartInventory = new SmartInventory
        {
            WarehouseId = dto.WarehouseId,
            SkuId = dto.SkuId,
            SupplierId = dto.SupplierId,
            LeadTimeDays = dto.LeadTimeDays,
            ReorderPoint = dto.ReorderPoint,
            TargetStock = dto.TargetStock,
            CoverageDays = dto.CoverageDays
        };

        var created = await repository.AddAsync(smartInventory, ct);

        // Reload with navigation properties for DTO mapping
        var createdWithRelations = await repository.GetByIdAsync(created.Id, ct);

        logger.LogInformation(
            "SmartInventory created [{SmartInventoryId}] [{WarehouseId}] [{SkuId}] [{SupplierId}]",
            created.Id, created.WarehouseId, created.SkuId, created.SupplierId);

        return Response<SmartInventoryDto>.Ok(MapToDto(createdWithRelations!), "SmartInventory created successfully");
    }

    public async Task<Response<SmartInventoryDto>> UpdateAsync(long id, UpdateSmartInventoryDto dto, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<SmartInventoryDto>();
        }

        var existing = await repository.GetByIdAsync(id, ct);
        if (existing is null || existing.Warehouse.CompanyId != companyId)
        {
            logger.LogWarning("Update SmartInventory failed: not found [{SmartInventoryId}] [{CompanyId}]", id,
                companyId);
            return Response<SmartInventoryDto>.Fail("SmartInventory not found", ErrorCode.NotFound);
        }

        // If SupplierId is being updated, validate it belongs to same company as warehouse
        if (dto.SupplierId.HasValue)
        {
            var supplier = await supplierRepository.GetByIdAsync(dto.SupplierId.Value, ct);
            if (supplier is null)
            {
                logger.LogWarning("Update SmartInventory failed: supplier not found [{SupplierId}]",
                    dto.SupplierId.Value);
                return Response<SmartInventoryDto>.Fail("Supplier not found", ErrorCode.NotFound);
            }

            if (existing.Warehouse.CompanyId != supplier.CompanyId)
            {
                logger.LogWarning(
                    "Update SmartInventory failed: Warehouse and Supplier must belong to same company [{WarehouseId}] [{SupplierId}]",
                    existing.WarehouseId, dto.SupplierId.Value);
                return Response<SmartInventoryDto>.Fail(
                    "Warehouse and Supplier must belong to same company", ErrorCode.BusinessRule);
            }

            existing.SupplierId = dto.SupplierId.Value;
        }

        if (dto.LeadTimeDays.HasValue)
        {
            existing.LeadTimeDays = dto.LeadTimeDays.Value;
        }

        if (dto.ReorderPoint.HasValue)
        {
            existing.ReorderPoint = dto.ReorderPoint.Value;
        }

        if (dto.TargetStock.HasValue)
        {
            existing.TargetStock = dto.TargetStock.Value;
        }

        if (dto.CoverageDays.HasValue)
        {
            existing.CoverageDays = dto.CoverageDays.Value;
        }

        await repository.UpdateAsync(existing, ct);

        // Reload with navigation properties for DTO mapping
        var updated = await repository.GetByIdAsync(id, ct);

        logger.LogInformation("SmartInventory updated [{SmartInventoryId}] [{CompanyId}]", id, companyId);

        return Response<SmartInventoryDto>.Ok(MapToDto(updated!), "SmartInventory updated successfully");
    }

    public async Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default)
    {
        var smartInventory = await repository.GetByIdAsync(id, ct);

        if (smartInventory is null || smartInventory.Warehouse.CompanyId != companyId)
        {
            logger.LogWarning("Delete SmartInventory failed: not found [{SmartInventoryId}] [{CompanyId}]", id,
                companyId);
            return Response<bool>.Fail("SmartInventory not found", ErrorCode.NotFound);
        }

        var warehouseCompanyId = smartInventory.Warehouse.CompanyId;

        await repository.DeleteAsync(id, ct);

        logger.LogInformation("SmartInventory deleted (soft-delete) [{SmartInventoryId}] [{CompanyId}]",
            id, warehouseCompanyId);

        return Response<bool>.Ok(true, "SmartInventory deleted successfully");
    }

    private static SmartInventoryDto MapToDto(SmartInventory smartInventory)
    {
        return new SmartInventoryDto(
            smartInventory.Id,
            smartInventory.Warehouse.CompanyId,
            smartInventory.WarehouseId,
            smartInventory.Warehouse.Name,
            smartInventory.SkuId,
            smartInventory.Sku.Barcode,
            smartInventory.SupplierId,
            smartInventory.Supplier.Name,
            smartInventory.LeadTimeDays,
            smartInventory.ReorderPoint,
            smartInventory.TargetStock,
            smartInventory.CoverageDays,
            smartInventory.CreatedAt,
            smartInventory.UpdatedAt);
    }
}
