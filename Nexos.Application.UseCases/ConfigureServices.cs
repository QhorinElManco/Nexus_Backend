using Microsoft.Extensions.DependencyInjection;
using Nexos.Application.Interfaces.UseCases;
using Nexos.Application.UseCases.Companies;
using Nexos.Application.Validator;

namespace Nexos.Application.UseCases;

public static class ConfigureServices
{
    public static void AddApplicationUseCasesServices(this IServiceCollection services)
    {
        // Registrar validadores desde su propio proyecto
        services.AddValidatorServices();
        services.AddScoped<ICompanyService, CompanyService>();
    }
}
