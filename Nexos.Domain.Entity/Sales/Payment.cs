using Nexos.Domain.Entity.Security;

namespace Nexos.Domain.Entity.Sales;

public class Payment : BaseEntity
{
    public required long CompanyId { get; set; }
    public required long OrderId { get; set; }
    public required long UserId { get; set; }
    public required decimal Amount { get; set; }
    public required string PaymentMethod { get; set; }
    public DateTime? CollectedAt { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    public Company Company { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public User User { get; set; } = null!;
}
