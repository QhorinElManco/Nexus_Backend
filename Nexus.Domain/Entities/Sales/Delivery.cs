using Nexus.Domain.Entities.Security;

namespace Nexus.Domain.Entities.Sales;

public class Delivery : BaseEntity
{
    public required long CompanyId { get; set; }
    public required long OrderId { get; set; }
    public required long UserId { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public double? DeliveryLat { get; set; }
    public double? DeliveryLng { get; set; }
    public required string Status { get; set; }
    public string? ProofOfDeliveryUrl { get; set; }

    public Company Company { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public User User { get; set; } = null!;
}
