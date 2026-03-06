namespace Nexos.Domain.Entities.Sales;

public class OrderDetail : BaseEntity
{
    public required long OrderId { get; set; }
    public required long SkuId { get; set; }
    public required int Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required decimal Subtotal { get; set; }

    public Order Order { get; set; } = null!;
    public Products.Sku Sku { get; set; } = null!;
}
