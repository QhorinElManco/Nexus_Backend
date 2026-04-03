using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Nexus.Api.Authorization;
using Nexus.Api.Extensions;
using Nexus.Api.Middleware;
using Nexus.Api.OpenApi;
using Nexus.Application;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Infrastructure;
using Serilog;
using Serilog.Context;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAppApiVersioning();
builder.Services.AddAppHealthChecks();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:Secret")
                                       ?? throw new InvalidOperationException("JWT Secret not configured")))
        };
    });

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("customers.view", p => p.RequireClaim("permission", "customers.view"))
    .AddPolicy("customers.manage", p => p.RequireClaim("permission", "customers.manage"))
    .AddPolicy("users.view", p => p.RequireClaim("permission", "users.view"))
    .AddPolicy("users.manage", p => p.RequireClaim("permission", "users.manage"))
    .AddPolicy("roles.view", p => p.RequireClaim("permission", "roles.view"))
    .AddPolicy("roles.manage", p => p.RequireClaim("permission", "roles.manage"))
    .AddPolicy("companies.view", p => p.RequireClaim("permission", "companies.view"))
    .AddPolicy("companies.manage", p => p.RequireClaim("permission", "companies.manage"))
    .AddPolicy("accesses.view", p => p.RequireClaim("permission", "accesses.view"))
    .AddPolicy("accesses.manage", p => p.RequireClaim("permission", "accesses.manage"))
    .AddPolicy("categories.view", p => p.RequireClaim("permission", "categories.view"))
    .AddPolicy("categories.manage", p => p.RequireClaim("permission", "categories.manage"))
    .AddPolicy("products.view", p => p.RequireClaim("permission", "products.view"))
    .AddPolicy("products.manage", p => p.RequireClaim("permission", "products.manage"))
    .AddPolicy("skus.view", p => p.RequireClaim("permission", "skus.view"))
    .AddPolicy("skus.manage", p => p.RequireClaim("permission", "skus.manage"))
    .AddPolicy("warehousetypes.view", p => p.RequireClaim("permission", "warehousetypes.view"))
    .AddPolicy("warehousetypes.manage", p => p.RequireClaim("permission", "warehousetypes.manage"))
    .AddPolicy("warehouses.view", p => p.RequireClaim("permission", "warehouses.view"))
    .AddPolicy("warehouses.manage", p => p.RequireClaim("permission", "warehouses.manage"))
    .AddPolicy("smartinventories.view", p => p.RequireClaim("permission", "smartinventories.view"))
    .AddPolicy("smartinventories.manage", p => p.RequireClaim("permission", "smartinventories.manage"))
    // Sales policies
    .AddPolicy("orders.view", p => p.RequireClaim("permission", "orders.view"))
    .AddPolicy("orders.manage", p => p.RequireClaim("permission", "orders.manage"))
    .AddPolicy("payments.view", p => p.RequireClaim("permission", "payments.view"))
    .AddPolicy("payments.manage", p => p.RequireClaim("permission", "payments.manage"))
    .AddPolicy("deliveries.view", p => p.RequireClaim("permission", "deliveries.view"))
    .AddPolicy("deliveries.manage", p => p.RequireClaim("permission", "deliveries.manage"))
    .AddPolicy("visits.view", p => p.RequireClaim("permission", "visits.view"))
    .AddPolicy("visits.manage", p => p.RequireClaim("permission", "visits.manage"))
    .AddPolicy("auth.logout", p => p.RequireAuthenticatedUser());

builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

builder.Host.UseSerilog();

builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddApplicationUseCasesServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseScalar();
}

app.Use(async (httpContext, next) =>
{
    var correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                        ?? Activity.Current?.Id
                        ?? Guid.NewGuid().ToString();

    httpContext.Response.Headers["X-Correlation-Id"] = correlationId;

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.MapAppHealthChecks();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<IsolationMiddleware>();
app.MapControllers();

var seedDataSettings = builder.Configuration.GetSection(SeedDataSettings.SectionName).Get<SeedDataSettings>();
if (seedDataSettings?.RunOnStartup == true)
{
    Log.Information("Data seed is enabled. Starting seed process...");
    using var scope = app.Services.CreateScope();
    var dataSeedService = scope.ServiceProvider.GetRequiredService<IDataSeedService>();
    await dataSeedService.SeedAsync();
    Log.Information("Data seed completed");
}

try
{
    Log.Information("Starting web application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
