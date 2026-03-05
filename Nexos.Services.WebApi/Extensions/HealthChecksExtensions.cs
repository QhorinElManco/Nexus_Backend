using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nexos.Services.WebApi.Extensions;

public static class HealthChecksExtensions
{
    public static IHealthChecksBuilder AddAppHealthChecks(this IServiceCollection services)
    {
        var builder = services.AddHealthChecks();

        builder.AddCheck("self", () => HealthCheckResult.Healthy("API is running"));

        return builder;
    }

    public static WebApplication MapAppHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"status\":\"" + report.Status + "\"}");
            }
        });

        app.MapHealthChecks("/healthz", new HealthCheckOptions { Predicate = _ => false });

        return app;
    }
}
