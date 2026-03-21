namespace Nexus.Application.Dto.Roles;

public record RoleDto(
    long Id,
    long CompanyId,
    string Name,
    string? Description,
    IReadOnlyList<PermissionDto> Permissions,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record PermissionDto(
    long Id,
    string Name,
    string? Description
);
