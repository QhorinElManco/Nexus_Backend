using Nexos.Domain.Entity.Security;

namespace Nexos.Domain.Entity.Audit;

public class AuditLog : BaseEntity
{
    public required long CompanyId { get; set; }
    public long? UserId { get; set; }
    public required string ModuleName { get; set; }
    public required string Action { get; set; }
    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public required string RiskLevel { get; set; }
    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    public Company Company { get; set; } = null!;
    public User? User { get; set; }
}
