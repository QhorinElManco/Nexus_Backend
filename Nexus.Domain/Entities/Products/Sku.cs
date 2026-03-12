using Nexus.Domain.Entities.Sales;
using Nexus.Domain.Entities.Transactions;

namespace Nexus.Domain.Entities.Products;

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
