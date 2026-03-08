using Nexos.Domain.Entity.Customers;
using Nexos.Domain.Entity.Products;
using Nexos.Domain.Entity.Security;

namespace Nexos.Domain.Entity.Sales;

public class Order : BaseEntity
{
    public required long CompanyId { get; set; }
    public required long CustomerId { get; set; }
    public required long UserId { get; set; }
    public long? VisitId { get; set; }
    public long? WarehouseId { get; set; }
    public required string OrderType { get; set; }
    public required string Status { get; set; }
    public required decimal TotalAmount { get; set; }

    public Company Company { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public User User { get; set; } = null!;
    public Visit? Visit { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Delivery> Deliveries { get; set; } = [];
}
