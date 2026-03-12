using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Security;

namespace Nexus.Infrastructure.Persistence.Configurations.Security;

public class PermissionConfiguration : BaseEntityConfiguration<Access>
{
    public override void Configure(EntityTypeBuilder<Access> builder)
    {
        base.Configure(builder);

        builder.ToTable("SystemPermissions");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.HasIndex(p => p.Name).IsUnique();
    }
}
