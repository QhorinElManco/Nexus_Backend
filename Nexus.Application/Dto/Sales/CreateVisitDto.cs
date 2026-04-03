namespace Nexus.Application.Dto.Sales;

public record CreateVisitDto(
    long CustomerId,
    double? CheckInLat,
    double? CheckInLng,
    string? Status
);
