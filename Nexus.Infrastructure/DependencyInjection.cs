using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Nexus.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Punto de entrada único de la capa Infrastructure.
    /// Registra todos los servicios de persistencia y logging transversal.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddPersistenceServices(configuration, environment);
        services.AddTransversalLoggingServices(configuration);

        return services;
    }
}
