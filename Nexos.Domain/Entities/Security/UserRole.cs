namespace Nexos.Domain.Entities.Security;

public class UserRole : BaseEntity
{
    public required long UserId { get; set; }
    public required long RoleId { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
