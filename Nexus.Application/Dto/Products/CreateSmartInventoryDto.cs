namespace Nexus.Application.Dto.Products;

public record CreateSmartInventoryDto(
    long WarehouseId,
    long SkuId,
    long SupplierId,
    int LeadTimeDays,
    int ReorderPoint,
    int TargetStock,
    int CoverageDays
);
