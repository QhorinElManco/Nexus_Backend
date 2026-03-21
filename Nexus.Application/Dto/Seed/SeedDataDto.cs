namespace Nexus.Application.Dto.Seed;

public record SeedDataDto(
    string CompanyName,
    SeedUserDto AdminUser,
    IReadOnlyList<SeedRoleDto> Roles
);

public record SeedUserDto(
    string Username,
    string Password,
    IReadOnlyList<string> Roles
);
