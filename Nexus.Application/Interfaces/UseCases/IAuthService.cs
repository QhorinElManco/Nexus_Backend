using Nexus.Application.Dto.Auth;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IAuthService
{
    Task<Response<LoginResultDto>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Response<LoginResultDto>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task<Response<bool>> LogoutAsync(long userId, CancellationToken ct = default);
}
