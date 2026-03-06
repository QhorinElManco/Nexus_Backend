using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexos.Domain.Entities.Products;

namespace Nexos.Persistence.Configurations.Products;

public class SkuConfiguration : BaseEntityConfiguration<Sku>
{
    public override void Configure(EntityTypeBuilder<Sku> builder)
    {
        base.Configure(builder);

        builder.ToTable("Skus");

        builder.Property(s => s.ProductId)
            .IsRequired();

        builder.Property(s => s.Barcode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.UnitMeasure)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.BasePrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(s => s.Barcode).IsUnique();
        builder.HasIndex(s => s.ProductId);

        builder.HasOne(s => s.Product)
            .WithMany(p => p.Skus)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
