using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Audit;
using Nexus.Domain.Entities.Customers;
using Nexus.Domain.Entities.Products;
using Nexus.Domain.Entities.Sales;
using Nexus.Domain.Entities.Security;
using Nexus.Domain.Entities.Transactions;

namespace Nexus.Infrastructure.Persistence;

public class NexusDbContext(DbContextOptions<NexusDbContext> options, IClaimsExtractor? claimsExtractor)
    : DbContext(options)
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
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<WarehouseType> WarehouseTypes => Set<WarehouseType>();
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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexusDbContext).Assembly);

        // Apply global query filters for multi-tenant isolation
        ApplyGlobalQueryFilters(modelBuilder);
    }

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Get current companyId from claims extractor
        long? companyId = null;
        var isSuperAdmin = false;

        try
        {
            isSuperAdmin = claimsExtractor?.IsSuperAdmin() ?? false;
            if (!isSuperAdmin)
            {
                companyId = claimsExtractor?.GetCurrentCompanyId();
            }
        }
        catch
        {
            // If claims extractor fails (not in HTTP context), skip filters
            // This allows seed operations to work
        }

        // If superadmin, skip all filters - can see everything
        if (isSuperAdmin)
        {
            return;
        }

        // If no companyId, apply global filtering
        if (companyId.HasValue)
        {
            // Products
            modelBuilder.Entity<Product>().HasQueryFilter(p => p.CompanyId == companyId);

            // Categories
            modelBuilder.Entity<Category>().HasQueryFilter(c => c.CompanyId == companyId);

            // WarehouseTypes
            modelBuilder.Entity<WarehouseType>().HasQueryFilter(wt => wt.CompanyId == companyId);

            // Warehouses
            modelBuilder.Entity<Warehouse>().HasQueryFilter(w => w.CompanyId == companyId);

            // Suppliers
            modelBuilder.Entity<Supplier>().HasQueryFilter(s => s.CompanyId == companyId);

            // Customers
            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.CompanyId == companyId);

            // Users
            modelBuilder.Entity<User>().HasQueryFilter(u => u.CompanyId == companyId);

            // Roles
            modelBuilder.Entity<Role>().HasQueryFilter(r => r.CompanyId == companyId);

            // Orders
            modelBuilder.Entity<Order>().HasQueryFilter(o => o.CompanyId == companyId);

            // Visits
            modelBuilder.Entity<Visit>().HasQueryFilter(v => v.CompanyId == companyId);

            // Payments
            modelBuilder.Entity<Payment>().HasQueryFilter(p => p.CompanyId == companyId);

            // Deliveries
            modelBuilder.Entity<Delivery>().HasQueryFilter(d => d.CompanyId == companyId);

            // KardexEntries
            modelBuilder.Entity<KardexEntry>().HasQueryFilter(k => k.CompanyId == companyId);

            // AuditLogs
            modelBuilder.Entity<AuditLog>().HasQueryFilter(a => a.CompanyId == companyId);

            // CustomerAssignments - filter by user's company through Customer
            modelBuilder.Entity<CustomerAssignment>().HasQueryFilter(ca =>
                claimsExtractor != null &&
                EF.Property<long>(ca.Customer, "CompanyId") == companyId);

            // SmartInventory - filter through Warehouse -> Company
            modelBuilder.Entity<SmartInventory>().HasQueryFilter(si =>
                si.Warehouse.CompanyId == companyId);

            // OrderDetails - filter through Order -> Company
            modelBuilder.Entity<OrderDetail>().HasQueryFilter(od =>
                od.Order.CompanyId == companyId);

            // UserRoles - filter by user's company
            modelBuilder.Entity<UserRole>().HasQueryFilter(ur =>
                ur.User.CompanyId == companyId);

            // RolePermissions - filter by Role -> Company
            modelBuilder.Entity<RoleAccess>().HasQueryFilter(rp =>
                rp.Role.CompanyId == companyId);

            // Skus - filter through Product -> Company
            modelBuilder.Entity<Sku>().HasQueryFilter(s =>
                s.Product.CompanyId == companyId);
        }
    }
}
