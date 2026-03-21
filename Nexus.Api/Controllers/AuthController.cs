using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexus.Application.Dto.Auth;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.UseCases;

namespace Nexus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<Response<LoginResultDto>>> Login([FromBody] LoginRequest request,
        CancellationToken ct = default)
    {
        var result = await authService.LoginAsync(request, ct);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<Response<LoginResultDto>>> Refresh([FromBody] RefreshTokenRequest request,
        CancellationToken ct = default)
    {
        var result = await authService.RefreshTokenAsync(request, ct);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("logout")]
    [Authorize(Policy = "auth.logout")]
    public async Task<ActionResult<Response<bool>>> Logout(CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(Response<bool>.Fail("Invalid token", ErrorCode.UnauthorizedAccess));
        }

        var result = await authService.LogoutAsync(userId, ct);
        return Ok(result);
    }
}
