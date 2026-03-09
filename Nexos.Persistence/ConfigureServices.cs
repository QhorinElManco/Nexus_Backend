using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Nexos.Persistence;

public static class ConfigureServices
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DefaultConnection' not found.");

        services.AddDbContext<NexosDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            // TODO: Habilitar el interceptor cuando ya tengamos compania y autenticación de usuarios
            // options.AddInterceptors(new SaveChangesInterceptor());

            // Habilitar el registro de datos sensibles solo en desarrollo
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Registro de la base de datos de revisión de salud
        services.AddHealthChecks()
            .AddDbContextCheck<NexosDbContext>("database");
    }
}
