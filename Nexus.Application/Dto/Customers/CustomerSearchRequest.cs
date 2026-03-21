namespace Nexus.Application.Dto.Customers;

public record CustomerSearchRequest(
    string? SearchTerm,
    long? CompanyId,
    string? Status,
    int Page = 1,
    int PageSize = 50
);
