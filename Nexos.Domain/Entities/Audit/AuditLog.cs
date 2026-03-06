namespace Nexos.Domain.Entities.Audit;

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

    public Security.Company Company { get; set; } = null!;
    public Security.User? User { get; set; }
}
