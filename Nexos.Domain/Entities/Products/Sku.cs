namespace Nexos.Domain.Entities.Products;

public class Sku : BaseEntity
{
    public required long ProductId { get; set; }
    public required string Barcode { get; set; }
    public required string UnitMeasure { get; set; }
    public required decimal BasePrice { get; set; }
    public bool IsActive { get; set; } = true;

    public Product Product { get; set; } = null!;
    public ICollection<SmartInventory> SmartInventories { get; set; } = [];
    public ICollection<Transactions.KardexEntry> KardexEntries { get; set; } = [];
    public ICollection<Sales.OrderDetail> OrderDetails { get; set; } = [];
}
