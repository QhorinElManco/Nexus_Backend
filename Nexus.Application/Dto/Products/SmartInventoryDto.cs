namespace Nexus.Application.Dto.Products;

public record SmartInventoryDto(
    long Id,
    long CompanyId,
    long WarehouseId,
    string? WarehouseName,
    long SkuId,
    string? SkuBarcode,
    long SupplierId,
    string? SupplierName,
    int LeadTimeDays,
    int ReorderPoint,
    int TargetStock,
    int CoverageDays,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
