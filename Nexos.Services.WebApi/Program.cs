using Nexos.Services.WebApi.Extensions;
using Nexos.Transversal.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAppApiVersioning();
builder.Services.AddAppHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddTransversalLoggingServices(builder.Configuration);
builder.Host.UseSerilog();

// Configure Entity Framework Core with PostgreSQL
// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
//                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//
// builder.Services.AddDbContext<NexosDbContext>(options =>
// {
//     options.UseNpgsql(connectionString);
//
//     // Enable sensitive data logging only in development
//     if (builder.Environment.IsDevelopment())
//     {
//         options.EnableSensitiveDataLogging();
//         options.EnableDetailedErrors();
//     }
// });

// Register database health check
// builder.Services.AddHealthChecks()
//     .AddDbContextCheck<NexosDbContext>("database");

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
