namespace Nexus.Application.Dto.Products;

public record CreateWarehouseDto(
    long ManagerId,
    string Name,
    long WarehouseTypeId,
    double? Lat = null,
    double? Lng = null
);
