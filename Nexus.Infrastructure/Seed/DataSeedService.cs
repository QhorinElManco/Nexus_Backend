using Microsoft.EntityFrameworkCore;
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
    IPasswordHasher passwordHasher) : IDataSeedService
{
    private static readonly List<SeedAccessDto> AccessDefinitions =
    [
        new SeedAccessDto("users.view", "Ver usuarios"),
        new SeedAccessDto("users.manage", "Crear, editar y eliminar usuarios"),
        new SeedAccessDto("roles.view", "Ver roles"),
        new SeedAccessDto("roles.manage", "Crear, editar y eliminar roles"),
        new SeedAccessDto("companies.view", "Ver compañías"),
        new SeedAccessDto("companies.manage", "Crear, editar y eliminar compañías"),
        new SeedAccessDto("customers.view", "Ver clientes"),
        new SeedAccessDto("customers.manage", "Crear, editar y eliminar clientes"),
        new SeedAccessDto("visits.view", "Ver visitas"),
        new SeedAccessDto("visits.manage", "Crear y editar visitas"),
        new SeedAccessDto("orders.view", "Ver pedidos"),
        new SeedAccessDto("orders.manage", "Crear, editar y eliminar pedidos"),
        new SeedAccessDto("payments.view", "Ver pagos"),
        new SeedAccessDto("payments.manage", "Registrar y gestionar pagos"),
        new SeedAccessDto("deliveries.view", "Ver entregas"),
        new SeedAccessDto("deliveries.manage", "Gestionar entregas"),
        new SeedAccessDto("products.view", "Ver productos"),
        new SeedAccessDto("products.manage", "Crear, editar y eliminar productos"),
        new SeedAccessDto("inventory.view", "Ver inventario"),
        new SeedAccessDto("inventory.manage", "Ajustar inventario"),
        new SeedAccessDto("warehouses.view", "Ver almacenes"),
        new SeedAccessDto("warehouses.manage", "Gestionar almacenes"),
        new SeedAccessDto("suppliers.view", "Ver proveedores"),
        new SeedAccessDto("suppliers.manage", "Gestionar proveedores"),
        new SeedAccessDto("reports.view", "Ver reportes"),
        new SeedAccessDto("audit.view", "Ver logs de auditoría"),
        new SeedAccessDto("accesses.view", "Ver permisos"),
        new SeedAccessDto("accesses.manage", "Crear, editar y eliminar permisos")
    ];

    private static readonly List<SeedRoleDto> RoleDefinitions =
    [
        new SeedRoleDto(
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
        new SeedRoleDto(
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
        new SeedRoleDto(
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
        new SeedRoleDto(
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
        new SeedRoleDto(
            "Finance",
            "Gestión financiera",
            ["orders.view", "payments.view", "payments.manage", "reports.view"]
        )
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedCompanyAsync(ct);
        await SeedPermissionsAsync(ct);
        await SeedRolesAsync(ct);
        await SeedAdminUserAsync(ct);
    }

    private async Task SeedCompanyAsync(CancellationToken ct)
    {
        var companyName = "Demo Company";
        var exists = await companyRepository.GetAllAsync(ct);
        if (exists.Any(c => c.Name == companyName))
        {
            return;
        }

        var company = new Company { Name = companyName, TaxId = "DEMO-001", IsActive = true };

        context.Companies.Add(company);
        await context.SaveChangesAsync(ct);
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        var existingNames = context.Permissions.Select(a => a.Name).ToList();
        var newPermissions = AccessDefinitions.Where(a => !existingNames.Contains(a.Name)).ToList();

        if (newPermissions.Count == 0)
        {
            return;
        }

        foreach (var access in newPermissions)
        {
            context.Permissions.Add(new Access { Name = access.Name, Description = access.Description });
        }

        await context.SaveChangesAsync(ct);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        var company = await context.Companies.FirstAsync(c => c.Name == "Demo Company", ct);
        var existingRoles = context.Roles.Where(r => r.CompanyId == company.Id).Select(r => r.Name).ToList();
        var newRoles = RoleDefinitions.Where(r => !existingRoles.Contains(r.Name)).ToList();

        if (newRoles.Count == 0)
        {
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
    }

    private async Task SeedAdminUserAsync(CancellationToken ct)
    {
        var company = await context.Companies.FirstAsync(c => c.Name == "Demo Company", ct);
        const string adminUsername = "admin";

        if (await userRepository.ExistsByUsernameAsync(adminUsername, ct: ct))
        {
            return;
        }

        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin" && r.CompanyId == company.Id, ct);

        var adminUser = new User
        {
            Username = adminUsername,
            PasswordHash =
                passwordHasher.Hash(Environment.GetEnvironmentVariable("NEXUS_ADMIN_PASSWORD") ?? "Admin123!"),
            FullName = "Administrator",
            CompanyId = company.Id,
            IsActive = true
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync(ct);

        context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });

        await context.SaveChangesAsync(ct);
    }
}
