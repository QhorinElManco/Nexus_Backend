using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Application.UseCases.Companies;

namespace Nexus.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los casos de uso y validadores de la capa Application.
    /// La persistencia y el logging son responsabilidad de Infrastructure.
    /// </summary>
    public static IServiceCollection AddApplicationUseCasesServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}
