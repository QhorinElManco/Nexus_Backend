namespace Nexus.Api.Middleware;

public class IsolationMiddleware(RequestDelegate next, ILogger<IsolationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip if not authenticated
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        // Extract company_id from JWT claims
        var companyIdClaim = context.User.FindFirst("company_id");
        if (companyIdClaim != null && long.TryParse(companyIdClaim.Value, out var companyId))
        {
            // Store in HttpContext.Items for downstream access
            context.Items["CompanyId"] = companyId;

            logger.LogDebug("Company isolation: Extracted company_id {CompanyId} from JWT for request to {Path}",
                companyId, context.Request.Path);
        }
        else if (companyIdClaim != null)
        {
            logger.LogWarning(
                "Invalid company_id claim value '{Value}' on {Path}. Claim value is not a valid number.",
                companyIdClaim.Value, context.Request.Path);
        }

        await next(context);
    }
}
