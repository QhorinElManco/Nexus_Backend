namespace Nexus.Application.Dto.Sales;

public record CreateDeliveryDto(
    long OrderId,
    string? Status
);
