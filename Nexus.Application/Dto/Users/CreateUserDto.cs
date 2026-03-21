namespace Nexus.Application.Dto.Users;

public record CreateUserDto(
    string Username,
    string Password,
    string FullName,
    long CompanyId
);
