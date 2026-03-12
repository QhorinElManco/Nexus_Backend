namespace Nexus.Domain.Entities.Security;

public class Role : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RoleAccess> RolePermissions { get; set; } = [];
}
