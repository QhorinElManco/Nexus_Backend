using Nexus.Domain.Entities.Customers;
using Nexus.Domain.Entities.Security;

namespace Nexus.Domain.Entities.Sales;

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

    public Company Company { get; set; } = null!;
    public User User { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = [];
}
