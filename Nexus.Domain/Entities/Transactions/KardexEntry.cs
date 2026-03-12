using Nexus.Domain.Entities.Products;
using Nexus.Domain.Entities.Security;

namespace Nexus.Domain.Entities.Transactions;

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

    public Company Company { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public Sku Sku { get; set; } = null!;
    public User User { get; set; } = null!;
}
