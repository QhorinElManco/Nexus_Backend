namespace Nexus.Application.Dto.Products;

public record SkuDto(
    long Id,
    long ProductId,
    string ProductName,
    string Barcode,
    string UnitMeasure,
    decimal BasePrice,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
