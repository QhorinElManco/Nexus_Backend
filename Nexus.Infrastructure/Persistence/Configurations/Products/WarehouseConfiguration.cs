using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Products;

namespace Nexus.Infrastructure.Persistence.Configurations.Products;

public class WarehouseConfiguration : BaseEntityConfiguration<Warehouse>
{
    public override void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        base.Configure(builder);

        builder.ToTable("Warehouses");

        builder.Property(w => w.CompanyId)
            .IsRequired();

        builder.Property(w => w.ManagerId)
            .IsRequired();

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.WarehouseTypeId)
            .IsRequired();

        builder.Property(w => w.Lat);

        builder.Property(w => w.Lng);

        builder.HasIndex(w => w.CompanyId);
        builder.HasIndex(w => new { w.CompanyId, w.Name }).IsUnique();

        builder.HasOne(w => w.Company)
            .WithMany(c => c.Warehouses)
            .HasForeignKey(w => w.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Manager)
            .WithMany(u => u.ManagedWarehouses)
            .HasForeignKey(w => w.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.WarehouseType)
            .WithMany(wt => wt.Warehouses)
            .HasForeignKey(w => w.WarehouseTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
