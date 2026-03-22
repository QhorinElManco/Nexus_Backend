namespace Nexus.Application.Dto.Products;

public record CreateCategoryDto(
    long CompanyId,
    string Name,
    string? Description
);
