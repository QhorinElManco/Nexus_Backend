using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace Nexos.Application.Validator;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationValidatorServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Companies.CreateCompanyDtoValidator>();
        return services;
    }
}
