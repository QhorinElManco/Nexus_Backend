namespace Nexus.Application.Dto.Products;

public record UpdateWarehouseDto(
    string Name,
    long? WarehouseTypeId = null,
    long? ManagerId = null,
    double? Lat = null,
    double? Lng = null
);
