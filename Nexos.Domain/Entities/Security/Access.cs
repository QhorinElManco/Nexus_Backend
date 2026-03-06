namespace Nexos.Domain.Entities.Security;

public class Access : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<RoleAccess> RolePermissions { get; set; } = [];
}
