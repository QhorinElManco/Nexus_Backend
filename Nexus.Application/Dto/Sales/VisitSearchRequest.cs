namespace Nexus.Application.Dto.Sales;

public record VisitSearchRequest(
    int Page = 1,
    int PageSize = 20,
    long? CustomerId = null,
    long? UserId = null,
    string? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
