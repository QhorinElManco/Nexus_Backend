using System.Globalization;
using Nexus.Api.Extensions;
using Nexus.Application;
using Nexus.Infrastructure;
using Serilog;

// Bootstrap logger: captura errores antes de que el DI container esté listo.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAppApiVersioning();
builder.Services.AddAppHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Host.UseSerilog();

// Infrastructure: persistencia (EF Core + repositorios) y logging transversal (Serilog).
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// Application: casos de uso y validadores.
builder.Services.AddApplicationUseCasesServices();

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
