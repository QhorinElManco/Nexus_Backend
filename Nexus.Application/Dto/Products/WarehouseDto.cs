namespace Nexus.Application.Dto.Products;

public record WarehouseDto(
    long Id,
    long CompanyId,
    string? CompanyName,
    long ManagerId,
    string? ManagerName,
    string Name,
    long WarehouseTypeId,
    string? WarehouseTypeName,
    double? Lat,
    double? Lng,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
