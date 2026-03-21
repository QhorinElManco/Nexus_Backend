using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Application.UseCases.Auth;
using Nexus.Application.UseCases.Companies;
using Nexus.Application.UseCases.Roles;
using Nexus.Application.UseCases.Users;

namespace Nexus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationUseCasesServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}
