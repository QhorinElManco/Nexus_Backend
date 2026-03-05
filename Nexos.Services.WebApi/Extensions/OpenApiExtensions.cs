using Scalar.AspNetCore;

namespace Nexos.Services.WebApi.Extensions;

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
