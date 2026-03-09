namespace Nexos.Application.Dto.Companies;

public record CompanySearchRequest(
    string? SearchTerm,
    int Page = 1,
    int PageSize = 50
);
