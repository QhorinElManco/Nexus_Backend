using Microsoft.EntityFrameworkCore;
using Nexos.Domain.Entities;
using Nexos.Domain.Entities.Security;
using Nexos.Domain.Entities.Customers;
using Nexos.Domain.Entities.Products;
using Nexos.Domain.Entities.Transactions;
using Nexos.Domain.Entities.Sales;
using Nexos.Domain.Entities.Audit;

namespace Nexos.Persistence;

public class NexosDbContext(DbContextOptions<NexosDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Access> Permissions => Set<Access>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RoleAccess> RolePermissions => Set<RoleAccess>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAssignment> CustomerAssignments => Set<CustomerAssignment>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Sku> Skus => Set<Sku>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<SmartInventory> SmartInventories => Set<SmartInventory>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<KardexEntry> KardexEntries => Set<KardexEntry>();

    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexosDbContext).Assembly);
    }
}
