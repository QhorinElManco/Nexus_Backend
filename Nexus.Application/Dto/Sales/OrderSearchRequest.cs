namespace Nexus.Application.Dto.Sales;

public record OrderSearchRequest(
    int Page = 1,
    int PageSize = 20,
    long? CustomerId = null,
    string? OrderType = null,
    string? Status = null,
    long? UserId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
