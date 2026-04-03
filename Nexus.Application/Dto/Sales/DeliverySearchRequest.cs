namespace Nexus.Application.Dto.Sales;

public record DeliverySearchRequest(
    int Page = 1,
    int PageSize = 20,
    long? OrderId = null,
    long? CompanyId = null,
    string? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);
