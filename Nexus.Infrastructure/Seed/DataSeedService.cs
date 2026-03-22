using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Seed;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Security;
using Nexus.Infrastructure.Persistence;

namespace Nexus.Infrastructure.Seed;

public class DataSeedService(
    NexusDbContext context,
    ICompanyRepository companyRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILogger<DataSeedService> logger) : IDataSeedService
{
    private static readonly List<SeedAccessDto> AccessDefinitions =
    [
        new("users.view", "Ver usuarios"),
        new("users.manage", "Crear, editar y eliminar usuarios"),
        new("roles.view", "Ver roles"),
        new("roles.manage", "Crear, editar y eliminar roles"),
        new("companies.view", "Ver compañías"),
        new("companies.manage", "Crear, editar y eliminar compañías"),
        new("customers.view", "Ver clientes"),
        new("customers.manage", "Crear, editar y eliminar clientes"),
        new("visits.view", "Ver visitas"),
        new("visits.manage", "Crear y editar visitas"),
        new("orders.view", "Ver pedidos"),
        new("orders.manage", "Crear, editar y eliminar pedidos"),
        new("payments.view", "Ver pagos"),
        new("payments.manage", "Registrar y gestionar pagos"),
        new("deliveries.view", "Ver entregas"),
        new("deliveries.manage", "Gestionar entregas"),
        new("products.view", "Ver productos"),
        new("products.manage", "Crear, editar y eliminar productos"),
        new("inventory.view", "Ver inventario"),
        new("inventory.manage", "Ajustar inventario"),
        new("warehouses.view", "Ver almacenes"),
        new("warehouses.manage", "Gestionar almacenes"),
        new("suppliers.view", "Ver proveedores"),
        new("suppliers.manage", "Gestionar proveedores"),
        new("reports.view", "Ver reportes"),
        new("audit.view", "Ver logs de auditoría"),
        new("accesses.view", "Ver permisos"),
        new("accesses.manage", "Crear, editar y eliminar permisos")
    ];

    private static readonly List<SeedRoleDto> RoleDefinitions =
    [
        new(
            "Admin",
            "Acceso total al sistema",
            [
                "users.view",
                "users.manage",
                "roles.view",
                "roles.manage",
                "companies.view",
                "companies.manage",
                "customers.view",
                "customers.manage",
                "visits.view",
                "visits.manage",
                "orders.view",
                "orders.manage",
                "payments.view",
                "payments.manage",
                "deliveries.view",
                "deliveries.manage",
                "products.view",
                "products.manage",
                "inventory.view",
                "inventory.manage",
                "warehouses.view",
                "warehouses.manage",
                "suppliers.view",
                "suppliers.manage",
                "reports.view",
                "audit.view",
                "accesses.view",
                "accesses.manage"
            ]
        ),
        new(
            "Manager",
            "Gestión operativa",
            [
                "customers.view",
                "customers.manage",
                "visits.view",
                "visits.manage",
                "orders.view",
                "orders.manage",
                "payments.view",
                "payments.manage",
                "deliveries.view",
                "deliveries.manage",
                "products.view",
                "products.manage",
                "inventory.view",
                "inventory.manage",
                "warehouses.view",
                "suppliers.view",
                "reports.view"
            ]
        ),
        new(
            "Sales",
            "Gestión de ventas y clientes",
            [
                "customers.view",
                "customers.manage",
                "visits.view",
                "visits.manage",
                "orders.view",
                "orders.manage",
                "payments.view",
                "deliveries.view"
            ]
        ),
        new(
            "Warehouse",
            "Gestión de inventario y entregas",
            [
                "products.view",
                "products.manage",
                "inventory.view",
                "inventory.manage",
                "warehouses.view",
                "warehouses.manage",
                "suppliers.view",
                "suppliers.manage",
                "deliveries.view",
                "deliveries.manage",
                "orders.view"
            ]
        ),
        new(
            "Finance",
            "Gestión financiera",
            ["orders.view", "payments.view", "payments.manage", "reports.view"]
        )
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Starting data seed process");
        await SeedCompanyAsync(ct);
        await SeedPermissionsAsync(ct);
        await SeedRolesAsync(ct);
        await SeedAdminUserAsync(ct);
        logger.LogInformation("Data seed process completed successfully");
    }

    private async Task SeedCompanyAsync(CancellationToken ct)
    {
        var companyName = "Demo Company";
        var exists = await companyRepository.GetAllAsync(ct);
        if (exists.Any(c => c.Name == companyName))
        {
            logger.LogWarning("Seed company: skipped - company [{CompanyName}] already exists", companyName);
            return;
        }

        var company = new Company { Name = companyName, TaxId = "DEMO-001", IsActive = true };

        context.Companies.Add(company);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Seed company: created [{CompanyName}] [{CompanyId}]", companyName, company.Id);
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        var existingNames = context.Permissions.Select(a => a.Name).ToList();
        var newPermissions = AccessDefinitions.Where(a => !existingNames.Contains(a.Name)).ToList();

        if (newPermissions.Count == 0)
        {
            logger.LogWarning("Seed permissions: skipped - all {TotalCount} permissions already exist",
                AccessDefinitions.Count);
            return;
        }

        foreach (var access in newPermissions)
        {
            context.Permissions.Add(new Access { Name = access.Name, Description = access.Description });
        }

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Seed permissions: created {Count} new permissions out of {TotalCount}",
            newPermissions.Count, AccessDefinitions.Count);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        var company = await context.Companies.FirstAsync(c => c.Name == "Demo Company", ct);
        var existingRoles = context.Roles.Where(r => r.CompanyId == company.Id).Select(r => r.Name).ToList();
        var newRoles = RoleDefinitions.Where(r => !existingRoles.Contains(r.Name)).ToList();

        if (newRoles.Count == 0)
        {
            logger.LogWarning("Seed roles: skipped - all {TotalCount} roles already exist", RoleDefinitions.Count);
            return;
        }

        var allPermissions = context.Permissions.ToDictionary(a => a.Name);

        foreach (var roleDef in newRoles)
        {
            var role = new Role { Name = roleDef.Name, Description = roleDef.Description, CompanyId = company.Id };

            context.Roles.Add(role);
            await context.SaveChangesAsync(ct);

            foreach (var permissionName in roleDef.Permissions)
            {
                if (allPermissions.TryGetValue(permissionName, out var permission))
                {
                    context.RolePermissions.Add(new RoleAccess { RoleId = role.Id, PermissionId = permission.Id });
                }
            }
        }

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Seed roles: created {Count} new roles out of {TotalCount}", newRoles.Count,
            RoleDefinitions.Count);
    }

    private async Task SeedAdminUserAsync(CancellationToken ct)
    {
        var company = await context.Companies.FirstAsync(c => c.Name == "Demo Company", ct);
        const string adminUsername = "admin";

        if (await userRepository.ExistsByUsernameAsync(adminUsername, ct: ct))
        {
            logger.LogWarning("Seed admin user: skipped - user [{Username}] already exists", adminUsername);
            return;
        }

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin" && r.CompanyId == company.Id, ct);
        var password = Environment.GetEnvironmentVariable("NEXUS_ADMIN_PASSWORD") ?? "Admin123!";
        var adminUser = new User
        {
            Username = adminUsername,
            PasswordHash = passwordHasher.Hash(password),
            FullName = "Administrator",
            CompanyId = company.Id,
            IsActive = true
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync(ct);

        context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Seed admin user: created [{Username}] [{UserId}] with Admin role", adminUsername,
            adminUser.Id);
    }
}
