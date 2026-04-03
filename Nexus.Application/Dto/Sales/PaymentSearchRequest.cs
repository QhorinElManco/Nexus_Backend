namespace Nexus.Application.Dto.Sales;

public record PaymentSearchRequest(
    int Page = 1,
    int PageSize = 20,
    long? OrderId = null,
    long? CompanyId = null,
    string? PaymentMethod = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
