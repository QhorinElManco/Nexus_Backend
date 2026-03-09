using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nexos.Application.UseCases.Companies;

namespace Nexos.Application.UseCases;

public static class ConfigureServices
{
    public static void AddApplicationUseCasesServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<ICompanyService, CompanyService>();
    }
}
