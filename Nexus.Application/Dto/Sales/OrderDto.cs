namespace Nexus.Application.Dto.Sales;

public record OrderDto(
    long Id,
    long CompanyId,
    long CustomerId,
    string CustomerName,
    long UserId,
    string UserFullName,
    long? VisitId,
    long? WarehouseId,
    string OrderType,
    string Status,
    decimal TotalAmount,
    IReadOnlyList<OrderDetailDto> OrderDetails,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record OrderDetailDto(
    long Id,
    long OrderId,
    long SkuId,
    string SkuCode,
    string SkuName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);

public record SimpleOrderDetailDto(
    long SkuId,
    int Quantity,
    decimal UnitPrice
);
