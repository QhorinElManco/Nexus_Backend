using Nexos.Domain.Entity.Audit;
using Nexos.Domain.Entity.Products;
using Nexos.Domain.Entity.Sales;
using Nexos.Domain.Entity.Transactions;

namespace Nexos.Domain.Entity.Security;

public class User : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<Warehouse> ManagedWarehouses { get; set; } = [];
    public ICollection<KardexEntry> KardexEntries { get; set; } = [];
    public ICollection<Visit> Visits { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Delivery> Deliveries { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
