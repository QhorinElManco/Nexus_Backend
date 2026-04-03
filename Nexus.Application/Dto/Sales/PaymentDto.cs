namespace Nexus.Application.Dto.Sales;

public record PaymentDto(
    long Id,
    long CompanyId,
    long OrderId,
    string OrderNumber,
    long UserId,
    string UserFullName,
    decimal Amount,
    string PaymentMethod,
    DateTime? CollectedAt,
    double? Lat,
    double? Lng,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
