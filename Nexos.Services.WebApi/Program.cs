using System.Globalization;
using Nexos.Application.UseCases;
using Nexos.Persistence;
using Nexos.Services.WebApi.Extensions;
using Nexos.Transversal.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAppApiVersioning();
builder.Services.AddAppHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Host.UseSerilog();

builder.Services.AddPersistenceServices(builder.Configuration, builder.Environment);
builder.Services.AddTransversalLoggingServices(builder.Configuration);
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
