using Scalar.AspNetCore;

namespace Nexus.Api.Extensions;

public static class OpenApiExtensions
{
    public static WebApplication UseScalar(this WebApplication app)
    {
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("Nexos API")
                .WithTheme(ScalarTheme.BluePlanet);
        });

        return app;
    }
}
