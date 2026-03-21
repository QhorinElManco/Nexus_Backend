using System.Globalization;
using Nexus.Api.Extensions;
using Nexus.Application;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAppApiVersioning();
builder.Services.AddAppHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Host.UseSerilog();

builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddApplicationUseCasesServices();

var app = builder.Build();

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

var seedDataSettings = builder.Configuration.GetSection(SeedDataSettings.SectionName).Get<SeedDataSettings>();
if (seedDataSettings?.RunOnStartup == true)
{
    using var scope = app.Services.CreateScope();
    var dataSeedService = scope.ServiceProvider.GetRequiredService<IDataSeedService>();
    await dataSeedService.SeedAsync();
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
