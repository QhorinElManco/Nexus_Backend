using Nexus.Application.Dto.Users;

namespace Nexus.Application.Dto.Auth;

public record LoginResultDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserDto User
);
