using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Nexus.Application.Interfaces.UseCases;

namespace Nexus.Application.UseCases.Auth;

public class ClaimsExtractor(IHttpContextAccessor httpContextAccessor) : IClaimsExtractor
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public long GetCurrentCompanyId()
    {
        var context = httpContextAccessor.HttpContext
                      ?? throw new InvalidOperationException("HttpContext is not available");

        // First, try to get from HttpContext.Items (set by middleware)
        if (context.Items["CompanyId"] is long companyIdFromItems)
        {
            return companyIdFromItems;
        }

        // Fall back to extracting from claims
        var companyIdClaim = User?.FindFirst("company_id")
                             ?? throw new InvalidOperationException("company_id claim not found");

        return !long.TryParse(companyIdClaim.Value, out var companyId)
            ? throw new InvalidOperationException("company_id claim is not a valid number")
            : companyId;
    }

    public long GetCurrentUserId()
    {
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)
                          ?? throw new InvalidOperationException("user_id claim not found");

        return !long.TryParse(userIdClaim.Value, out var userId)
            ? throw new InvalidOperationException("user_id claim is not a valid number")
            : userId;
    }

    public IReadOnlyList<string> GetCurrentRoles()
    {
        return User?.FindAll(ClaimTypes.Role)
                   .Select(c => c.Value)
                   .ToList()
               ?? [];
    }

    public IReadOnlyList<string> GetCurrentPermissions()
    {
        return User?.FindAll("permission")
                   .Select(c => c.Value)
                   .ToList()
               ?? [];
    }

    public bool TryGetCurrentCompanyId(out long companyId)
    {
        try
        {
            companyId = GetCurrentCompanyId();
            return true;
        }
        catch
        {
            companyId = 0;
            return false;
        }
    }
}
