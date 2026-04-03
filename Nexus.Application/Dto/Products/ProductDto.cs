namespace Nexus.Application.Dto.Products;

public record ProductDto(
    long Id,
    long CompanyId,
    long? CategoryId,
    string? CategoryName,
    string Name,
    string? Brand,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
