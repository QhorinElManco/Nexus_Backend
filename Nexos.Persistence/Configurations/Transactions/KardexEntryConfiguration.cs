using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexos.Domain.Entities.Transactions;

namespace Nexos.Persistence.Configurations.Transactions;

public class KardexEntryConfiguration : BaseEntityConfiguration<KardexEntry>
{
    public override void Configure(EntityTypeBuilder<KardexEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("KardexEntries");

        builder.Property(k => k.CompanyId)
            .IsRequired();

        builder.Property(k => k.WarehouseId)
            .IsRequired();

        builder.Property(k => k.SkuId)
            .IsRequired();

        builder.Property(k => k.UserId)
            .IsRequired();

        builder.Property(k => k.TransactionType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(k => k.Quantity)
            .IsRequired();

        builder.Property(k => k.ReferenceDocType)
            .HasMaxLength(50);

        builder.Property(k => k.ReferenceDocId)
            .HasMaxLength(100);

        builder.Property(k => k.StockBefore)
            .IsRequired();

        builder.Property(k => k.StockAfter)
            .IsRequired();

        builder.Property(k => k.DeviceId)
            .HasMaxLength(100);

        builder.Property(k => k.Lat);

        builder.Property(k => k.Lng);

        builder.HasIndex(k => k.CompanyId);
        builder.HasIndex(k => k.WarehouseId);
        builder.HasIndex(k => k.SkuId);
        builder.HasIndex(k => k.CreatedAt);
        builder.HasIndex(k => new { k.CompanyId, k.CreatedAt });

        builder.HasOne(k => k.Company)
            .WithMany(c => c.KardexEntries)
            .HasForeignKey(k => k.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(k => k.Warehouse)
            .WithMany(w => w.KardexEntries)
            .HasForeignKey(k => k.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(k => k.Sku)
            .WithMany(s => s.KardexEntries)
            .HasForeignKey(k => k.SkuId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(k => k.User)
            .WithMany(u => u.KardexEntries)
            .HasForeignKey(k => k.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
