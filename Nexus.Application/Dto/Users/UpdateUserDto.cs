namespace Nexus.Application.Dto.Users;

public record UpdateUserDto(
    string FullName,
    bool IsActive
);
