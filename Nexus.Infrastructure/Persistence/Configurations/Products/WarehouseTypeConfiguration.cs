using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Products;

namespace Nexus.Infrastructure.Persistence.Configurations.Products;

public class WarehouseTypeConfiguration : BaseEntityConfiguration<WarehouseType>
{
    public override void Configure(EntityTypeBuilder<WarehouseType> builder)
    {
        base.Configure(builder);

        builder.ToTable("WarehouseTypes");

        builder.Property(wt => wt.CompanyId)
            .IsRequired();

        builder.Property(wt => wt.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(wt => wt.Description)
            .HasMaxLength(500);

        builder.HasIndex(wt => wt.CompanyId);
        builder.HasIndex(wt => new { wt.CompanyId, wt.Name }).IsUnique();

        builder.HasOne(wt => wt.Company)
            .WithMany(c => c.WarehouseTypes)
            .HasForeignKey(wt => wt.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
