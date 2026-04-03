namespace Nexus.Application.Dto.Sales;

public record DeliveryDto(
    long Id,
    long CompanyId,
    long OrderId,
    string OrderNumber,
    long UserId,
    string UserFullName,
    DateTime? DeliveryTime,
    double? DeliveryLat,
    double? DeliveryLng,
    string Status,
    string? ProofOfDeliveryUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
