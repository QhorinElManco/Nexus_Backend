namespace Nexus.Application.Dto.Sales;

public record CreatePaymentDto(
    long OrderId,
    decimal Amount,
    string PaymentMethod,
    double? Lat,
    double? Lng
);
