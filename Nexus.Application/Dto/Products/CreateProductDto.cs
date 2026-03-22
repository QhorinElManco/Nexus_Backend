namespace Nexus.Application.Dto.Products;

public record CreateProductDto(
    long CompanyId,
    long? CategoryId,
    string Name,
    string? Brand
);