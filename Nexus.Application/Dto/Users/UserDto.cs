namespace Nexus.Application.Dto.Users;

public record UserDto(
    long Id,
    string Username,
    string FullName,
    long CompanyId,
    string CompanyName,
    bool IsActive,
    IReadOnlyList<RoleDto> Roles,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record RoleDto(
    long Id,
    string Name,
    string? Description
);
