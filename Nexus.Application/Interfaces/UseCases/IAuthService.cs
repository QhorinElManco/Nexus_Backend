using Nexus.Application.Dto.Auth;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IAuthService
{
    public Task<Response<LoginResultDto>> LoginAsync(LoginRequest request, CancellationToken ct = default);

    public Task<Response<LoginResultDto>>
        RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);

    public Task<Response<bool>> LogoutAsync(long userId, CancellationToken ct = default);
}
