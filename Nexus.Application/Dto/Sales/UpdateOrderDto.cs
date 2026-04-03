namespace Nexus.Application.Dto.Sales;

public record UpdateOrderDto(
    string? OrderType,
    string? Status,
    long? VisitId,
    long? WarehouseId
);
