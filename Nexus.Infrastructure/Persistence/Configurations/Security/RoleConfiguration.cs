using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Security;

namespace Nexus.Infrastructure.Persistence.Configurations.Security;

public class RoleConfiguration : BaseEntityConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("Roles");

        builder.Property(r => r.CompanyId)
            .IsRequired();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.HasIndex(r => new { r.CompanyId, r.Name }).IsUnique();

        builder.HasOne(r => r.Company)
            .WithMany(c => c.Roles)
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
