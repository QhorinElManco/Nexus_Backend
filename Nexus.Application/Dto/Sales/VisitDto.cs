namespace Nexus.Application.Dto.Sales;

public record VisitDto(
    long Id,
    long CompanyId,
    long UserId,
    string UserFullName,
    long CustomerId,
    string CustomerName,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    double? CheckInLat,
    double? CheckInLng,
    string Status,
    string? CancelReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
