namespace Nexus.Application.Dto.Sales;

public record UpdateVisitDto(
    string? Status,
    string? CancelReason,
    string? Notes
);
