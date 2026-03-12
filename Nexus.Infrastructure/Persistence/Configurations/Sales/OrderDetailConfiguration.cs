using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Infrastructure.Persistence.Configurations.Sales;

public class OrderDetailConfiguration : BaseEntityConfiguration<OrderDetail>
{
    public override void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        base.Configure(builder);

        builder.ToTable("OrderDetails");

        builder.Property(od => od.OrderId)
            .IsRequired();

        builder.Property(od => od.SkuId)
            .IsRequired();

        builder.Property(od => od.Quantity)
            .IsRequired();

        builder.Property(od => od.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(od => od.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasIndex(od => od.OrderId);

        builder.HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(od => od.Sku)
            .WithMany(s => s.OrderDetails)
            .HasForeignKey(od => od.SkuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
