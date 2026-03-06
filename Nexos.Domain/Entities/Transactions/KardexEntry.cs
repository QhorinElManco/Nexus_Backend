namespace Nexos.Domain.Entities.Transactions;

public class KardexEntry : BaseEntity
{
    public required long CompanyId { get; set; }
    public required long WarehouseId { get; set; }
    public required long SkuId { get; set; }
    public required long UserId { get; set; }
    public required string TransactionType { get; set; }
    public required int Quantity { get; set; }
    public string? ReferenceDocType { get; set; }
    public string? ReferenceDocId { get; set; }
    public required int StockBefore { get; set; }
    public required int StockAfter { get; set; }
    public string? DeviceId { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    public Security.Company Company { get; set; } = null!;
    public Products.Warehouse Warehouse { get; set; } = null!;
    public Products.Sku Sku { get; set; } = null!;
    public Security.User User { get; set; } = null!;
}
