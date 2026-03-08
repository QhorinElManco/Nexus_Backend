using Nexos.Domain.Entity.Sales;
using Nexos.Domain.Entity.Security;
using Nexos.Domain.Entity.Transactions;

namespace Nexos.Domain.Entity.Products;

public class Warehouse : BaseEntity
{
    public required long CompanyId { get; set; }
    public required long ManagerId { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    public Company Company { get; set; } = null!;
    public User Manager { get; set; } = null!;
    public ICollection<SmartInventory> SmartInventories { get; set; } = [];
    public ICollection<KardexEntry> KardexEntries { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}
