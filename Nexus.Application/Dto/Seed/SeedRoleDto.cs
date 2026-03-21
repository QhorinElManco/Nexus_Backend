namespace Nexus.Application.Dto.Seed;

public record SeedRoleDto(
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions
);
