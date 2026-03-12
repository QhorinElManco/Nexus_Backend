using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Security;

namespace Nexus.Infrastructure.Persistence.Configurations.Security;

public class RoleAccessConfiguration : BaseEntityConfiguration<RoleAccess>
{
    public override void Configure(EntityTypeBuilder<RoleAccess> builder)
    {
        base.Configure(builder);

        builder.ToTable("RoleAccess");

        builder.Property(rp => rp.RoleId)
            .IsRequired();

        builder.Property(rp => rp.PermissionId)
            .IsRequired();

        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
