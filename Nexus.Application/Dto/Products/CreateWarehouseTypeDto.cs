namespace Nexus.Application.Dto.Products;

public record CreateWarehouseTypeDto(
    long CompanyId,
    string Name,
    string? Description
);
