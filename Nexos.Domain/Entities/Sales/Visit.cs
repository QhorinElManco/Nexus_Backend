namespace Nexos.Domain.Entities.Sales;

public class Visit : BaseEntity
{
    public required long CompanyId { get; set; }
    public required long UserId { get; set; }
    public required long CustomerId { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public double? CheckInLat { get; set; }
    public double? CheckInLng { get; set; }
    public required string Status { get; set; }
    public string? CancelReason { get; set; }

    public Security.Company Company { get; set; } = null!;
    public Security.User User { get; set; } = null!;
    public Customers.Customer Customer { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = [];
}
