namespace Nexus.Application.Dto.Products;

public record UpdateProductDto(
    long? CategoryId,
    string Name,
    string? Brand
);