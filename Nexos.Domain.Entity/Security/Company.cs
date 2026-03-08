using Nexos.Domain.Entity.Audit;
using Nexos.Domain.Entity.Customers;
using Nexos.Domain.Entity.Products;
using Nexos.Domain.Entity.Sales;
using Nexos.Domain.Entity.Transactions;

namespace Nexos.Domain.Entity.Security;

public class Company : BaseEntity
{
    public required string Name { get; set; }
    public required string TaxId { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Role> Roles { get; set; } = [];
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<Warehouse> Warehouses { get; set; } = [];
    public ICollection<Supplier> Suppliers { get; set; } = [];
    public ICollection<KardexEntry> KardexEntries { get; set; } = [];
    public ICollection<Visit> Visits { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Delivery> Deliveries { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
