namespace Nexos.Domain.Entities.Security;

public class User : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<Products.Warehouse> ManagedWarehouses { get; set; } = [];
    public ICollection<Transactions.KardexEntry> KardexEntries { get; set; } = [];
    public ICollection<Sales.Visit> Visits { get; set; } = [];
    public ICollection<Sales.Order> Orders { get; set; } = [];
    public ICollection<Sales.Payment> Payments { get; set; } = [];
    public ICollection<Sales.Delivery> Deliveries { get; set; } = [];
    public ICollection<Audit.AuditLog> AuditLogs { get; set; } = [];
}
