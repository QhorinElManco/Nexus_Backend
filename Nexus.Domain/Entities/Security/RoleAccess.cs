namespace Nexus.Domain.Entities.Security;

public class RoleAccess : BaseEntity
{
    public required long RoleId { get; set; }
    public required long PermissionId { get; set; }

    public Role Role { get; set; } = null!;
    public Access Permission { get; set; } = null!;
}
