using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Application.UseCases.Companies;

namespace Nexus.Application;

public static class ConfigureServices
{
    public static void AddApplicationUseCasesServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<ICompanyService, CompanyService>();
    }
}
