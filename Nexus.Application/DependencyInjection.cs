using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Application.UseCases.Access;
using Nexus.Application.UseCases.Auth;
using Nexus.Application.UseCases.Categories;
using Nexus.Application.UseCases.Companies;
using Nexus.Application.UseCases.Customers;
using Nexus.Application.UseCases.Products;
using Nexus.Application.UseCases.Roles;
using Nexus.Application.UseCases.Suppliers;
using Nexus.Application.UseCases.Users;

namespace Nexus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationUseCasesServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAccessService, AccessService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISkuService, SkuService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISupplierService, SupplierService>();

        return services;
    }
}
