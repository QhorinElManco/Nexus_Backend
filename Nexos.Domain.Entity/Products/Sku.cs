using Nexos.Domain.Entity.Sales;
using Nexos.Domain.Entity.Transactions;

namespace Nexos.Domain.Entity.Products;

public class Sku : BaseEntity
{
    public required long ProductId { get; set; }
    public required string Barcode { get; set; }
    public required string UnitMeasure { get; set; }
    public required decimal BasePrice { get; set; }
    public bool IsActive { get; set; } = true;

    public Product Product { get; set; } = null!;
    public ICollection<SmartInventory> SmartInventories { get; set; } = [];
    public ICollection<KardexEntry> KardexEntries { get; set; } = [];
    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
}
