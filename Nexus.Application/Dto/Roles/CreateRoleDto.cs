namespace Nexus.Application.Dto.Roles;

public record CreateRoleDto(
    string Name,
    string? Description,
    long CompanyId
);
