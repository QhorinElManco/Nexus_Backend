namespace Nexus.Application.Dto.Products;

public record CreateSkuDto(
    long ProductId,
    string Barcode,
    string UnitMeasure,
    decimal BasePrice
);