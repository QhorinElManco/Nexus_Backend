using Serilog;
using Nexos.Services.WebApi.Extensions;

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
