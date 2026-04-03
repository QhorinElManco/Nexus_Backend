namespace Nexus.Application.Dto.Sales;

public record UpdateDeliveryDto(
    string? Status,
    DateTime? DeliveryTime,
    double? DeliveryLat,
    double? DeliveryLng,
    string? ProofOfDeliveryUrl
);
