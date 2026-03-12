using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Infrastructure.Persistence;
using Nexus.Infrastructure.Persistence.Repositories;

namespace Nexus.Infrastructure;

internal static class PersistenceExtensions
{
    internal static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration,
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

        // Repositories
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        // Registro de la base de datos de revisión de salud
        services.AddHealthChecks()
            .AddDbContextCheck<NexosDbContext>("database");

        return services;
    }
}
