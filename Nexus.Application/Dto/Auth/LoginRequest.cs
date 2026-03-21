namespace Nexus.Application.Dto.Auth;

public record LoginRequest(
    string Username,
    string Password
);
