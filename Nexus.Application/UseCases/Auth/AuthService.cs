using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nexus.Application.Dto.Auth;
using Nexus.Application.Dto.Response;
using Nexus.Application.Dto.Users;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Security;

namespace Nexus.Application.UseCases.Auth;

public class AuthService(
    IUserRepository userRepository,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshTokenRequest> refreshTokenValidator,
    IPasswordHasher passwordHasher,
    IOptions<JwtSettings> jwtSettings) : IAuthService
{
    public async Task<Response<LoginResultDto>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validationResult = await loginValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<LoginResultDto>();
        }

        var user = await userRepository.GetByUsernameWithRolesAsync(request.Username, ct);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Response<LoginResultDto>.Fail("Invalid credentials", ErrorCode.UnauthorizedAccess);
        }

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        var loginResult = new LoginResultDto(
            accessToken,
            refreshToken,
            jwtSettings.Value.AccessTokenExpirationMinutes * 60,
            MapToUserDto(user)
        );

        return Response<LoginResultDto>.Ok(loginResult);
    }

    public async Task<Response<LoginResultDto>> RefreshTokenAsync(RefreshTokenRequest request,
        CancellationToken ct = default)
    {
        var validationResult = await refreshTokenValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<LoginResultDto>();
        }

        var principal = GetPrincipalFromToken(request.RefreshToken);

        if (principal is null)
        {
            return Response<LoginResultDto>.Fail("Invalid token", ErrorCode.UnauthorizedAccess);
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out _))
        {
            return Response<LoginResultDto>.Fail("Invalid token", ErrorCode.UnauthorizedAccess);
        }

        var user = await userRepository.GetByUsernameWithRolesAsync(principal.Identity?.Name ?? string.Empty, ct);
        if (user is null || !user.IsActive)
        {
            return Response<LoginResultDto>.Fail("User not found or inactive", ErrorCode.UnauthorizedAccess);
        }

        var newAccessToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        var loginResult = new LoginResultDto(
            newAccessToken,
            newRefreshToken,
            jwtSettings.Value.AccessTokenExpirationMinutes * 60,
            MapToUserDto(user)
        );

        return Response<LoginResultDto>.Ok(loginResult);
    }

    public Task<Response<bool>> LogoutAsync(long userId, CancellationToken ct = default)
    {
        return Task.FromResult(Response<bool>.Ok(true));
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.FullName),
            new("company_id", user.CompanyId.ToString(CultureInfo.InvariantCulture))
        };

        claims.AddRange(user.UserRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole.Role.Name)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Value.Issuer,
            audience: jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static UserDto MapToUserDto(User user) => new(
        user.Id,
        user.Username,
        user.FullName,
        user.CompanyId,
        string.Empty,
        user.IsActive,
        user.UserRoles.Select(ur => new RoleDto(ur.Role.Id, ur.Role.Name, ur.Role.Description)).ToList(),
        user.CreatedAt,
        user.UpdatedAt
    );
}
