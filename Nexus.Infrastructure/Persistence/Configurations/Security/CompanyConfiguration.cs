using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Security;

namespace Nexus.Infrastructure.Persistence.Configurations.Security;

public class CompanyConfiguration : BaseEntityConfiguration<Company>
{
    public override void Configure(EntityTypeBuilder<Company> builder)
    {
        base.Configure(builder);

        builder.ToTable("Companies");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.TaxId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(c => c.TaxId).IsUnique();
    }
}
