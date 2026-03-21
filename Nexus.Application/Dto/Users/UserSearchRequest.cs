namespace Nexus.Application.Dto.Users;

public record UserSearchRequest(
    string? SearchTerm,
    long? CompanyId,
    int Page = 1,
    int PageSize = 50
);
