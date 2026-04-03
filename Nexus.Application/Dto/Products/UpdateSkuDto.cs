namespace Nexus.Application.Dto.Products;

public record UpdateSkuDto(
    string Barcode,
    string UnitMeasure,
    decimal BasePrice
);
