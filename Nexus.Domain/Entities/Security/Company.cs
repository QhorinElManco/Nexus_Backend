using Nexus.Domain.Entities.Audit;
using Nexus.Domain.Entities.Customers;
using Nexus.Domain.Entities.Products;
using Nexus.Domain.Entities.Sales;
using Nexus.Domain.Entities.Transactions;

namespace Nexus.Domain.Entities.Security;

public class Company : BaseEntity
{
    public required string Name { get; set; }
    public required string TaxId { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<Warehouse> Warehouses { get; set; } = [];
    public ICollection<Supplier> Suppliers { get; set; } = [];
    public ICollection<KardexEntry> KardexEntries { get; set; } = [];
    public ICollection<Visit> Visits { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Delivery> Deliveries { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
