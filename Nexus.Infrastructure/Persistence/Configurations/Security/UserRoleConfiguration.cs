using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Security;

namespace Nexus.Infrastructure.Persistence.Configurations.Security;

public class UserRoleConfiguration : BaseEntityConfiguration<UserRole>
{
    public override void Configure(EntityTypeBuilder<UserRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserRoles");

        builder.Property(ur => ur.UserId)
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .IsRequired();

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
