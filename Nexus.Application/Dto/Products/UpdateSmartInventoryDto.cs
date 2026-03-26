namespace Nexus.Application.Dto.Products;

public record UpdateSmartInventoryDto(
    long? SupplierId,
    int? LeadTimeDays,
    int? ReorderPoint,
    int? TargetStock,
    int? CoverageDays
);
