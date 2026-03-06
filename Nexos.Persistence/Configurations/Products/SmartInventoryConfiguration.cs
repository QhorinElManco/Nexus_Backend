using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexos.Domain.Entities.Products;

namespace Nexos.Persistence.Configurations.Products;

public class SmartInventoryConfiguration : BaseEntityConfiguration<SmartInventory>
{
    public override void Configure(EntityTypeBuilder<SmartInventory> builder)
    {
        base.Configure(builder);

        builder.ToTable("SmartInventories");

        builder.Property(si => si.WarehouseId)
            .IsRequired();

        builder.Property(si => si.SkuId)
            .IsRequired();

        builder.Property(si => si.SupplierId)
            .IsRequired();

        builder.Property(si => si.LeadTimeDays)
            .IsRequired();

        builder.Property(si => si.ReorderPoint)
            .IsRequired();

        builder.Property(si => si.TargetStock)
            .IsRequired();

        builder.Property(si => si.CoverageDays)
            .IsRequired();

        builder.HasIndex(si => new { si.WarehouseId, si.SkuId }).IsUnique();

        builder.HasOne(si => si.Warehouse)
            .WithMany(w => w.SmartInventories)
            .HasForeignKey(si => si.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.Sku)
            .WithMany(s => s.SmartInventories)
            .HasForeignKey(si => si.SkuId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.Supplier)
            .WithMany(s => s.SmartInventories)
            .HasForeignKey(si => si.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
