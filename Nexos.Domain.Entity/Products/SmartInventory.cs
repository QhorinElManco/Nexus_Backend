namespace Nexos.Domain.Entity.Products;

public class SmartInventory : BaseEntity
{
    public required long WarehouseId { get; set; }
    public required long SkuId { get; set; }
    public required long SupplierId { get; set; }
    public required int LeadTimeDays { get; set; }
    public required int ReorderPoint { get; set; }
    public required int TargetStock { get; set; }
    public required int CoverageDays { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public Sku Sku { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
}
