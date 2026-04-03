using Nexus.Domain.Entities.Audit;
using Nexus.Domain.Entities.Products;
using Nexus.Domain.Entities.Sales;
using Nexus.Domain.Entities.Transactions;

namespace Nexus.Domain.Entities.Security;

public class User : BaseEntity
{
    public long? CompanyId { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public bool IsActive { get; set; } = true;

    public Company? Company { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<Warehouse> ManagedWarehouses { get; set; } = [];
    public ICollection<KardexEntry> KardexEntries { get; set; } = [];
    public ICollection<Visit> Visits { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Delivery> Deliveries { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}
