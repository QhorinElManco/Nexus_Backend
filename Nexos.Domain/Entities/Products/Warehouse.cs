namespace Nexos.Domain.Entities.Products;

public class Warehouse : BaseEntity
{
    public required long CompanyId { get; set; }
    public required long ManagerId { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    public Security.Company Company { get; set; } = null!;
    public Security.User Manager { get; set; } = null!;
    public ICollection<SmartInventory> SmartInventories { get; set; } = [];
    public ICollection<Transactions.KardexEntry> KardexEntries { get; set; } = [];
    public ICollection<Sales.Order> Orders { get; set; } = [];
}
