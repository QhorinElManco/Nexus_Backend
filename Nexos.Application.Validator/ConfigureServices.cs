using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Nexos.Application.Validator;

public static class ConfigureServices
{
    /// <summary>
    /// Registra los validadores FluentValidation definidos en este ensamblado.
    /// </summary>
    public static IServiceCollection AddValidatorServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }
}
