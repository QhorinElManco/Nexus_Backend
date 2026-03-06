using Serilog;
using Nexos.Services.WebApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Nexos.Persistence;

Log.Logger = SerilogExtensions.CreateBootstrapLogger().CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddAppSerilog(builder.Configuration);

    // Add services to the container.

    builder.Services.AddControllers();
    builder.Services.AddAppApiVersioning();
    builder.Services.AddAppHealthChecks();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();

    // Configure Entity Framework Core with PostgreSQL
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<NexosDbContext>(options =>
    {
        options.UseNpgsql(connectionString);

        // Enable sensitive data logging only in development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // Register database health check
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<NexosDbContext>("database");

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseScalar();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.MapAppHealthChecks();

    app.UseAuthorization();

    app.MapControllers();

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
