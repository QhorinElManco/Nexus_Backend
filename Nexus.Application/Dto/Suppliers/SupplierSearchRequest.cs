namespace Nexus.Application.Dto.Suppliers;

public record SupplierSearchRequest(
    long CompanyId,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 50
);
