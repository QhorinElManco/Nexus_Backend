namespace Nexus.Application.Dto.Products;

public record WarehouseTypeDto(
    long Id,
    long CompanyId,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
