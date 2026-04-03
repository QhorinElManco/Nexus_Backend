namespace Nexus.Application.Dto.Products;

public record CreateProductDto(
    long? CategoryId,
    string Name,
    string? Brand
);
