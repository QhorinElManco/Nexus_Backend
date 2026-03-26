using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Infrastructure.Persistence;
using Nexus.Infrastructure.Persistence.Repositories;
using Nexus.Infrastructure.Seed;
using Nexus.Infrastructure.Services;

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

        services.AddDbContext<NexusDbContext>(options =>
        {
            options.UseNpgsql(connectionString);

            if (!environment.IsDevelopment())
            {
                return;
            }

            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        services.AddScoped<IAccessRepository, AccessRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISkuRepository, SkuRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IWarehouseTypeRepository, WarehouseTypeRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<ISmartInventoryRepository, SmartInventoryRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IDataSeedService, DataSeedService>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<SeedDataSettings>(configuration.GetSection(SeedDataSettings.SectionName));

        services.AddHealthChecks()
            .AddDbContextCheck<NexusDbContext>("database");

        return services;
    }
}
