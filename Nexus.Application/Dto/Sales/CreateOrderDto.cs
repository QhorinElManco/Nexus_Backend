namespace Nexus.Application.Dto.Sales;

public record CreateOrderDto(
    long CustomerId,
    string OrderType,
    string? Status,
    long? VisitId,
    long? WarehouseId,
    IReadOnlyList<CreateOrderDetailDto>? OrderDetails
);

public record CreateOrderDetailDto(
    long SkuId,
    int Quantity,
    decimal UnitPrice
);
