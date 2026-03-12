using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Infrastructure.Persistence.Configurations.Sales;

public class OrderConfiguration : BaseEntityConfiguration<Order>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        base.Configure(builder);

        builder.ToTable("Orders");

        builder.Property(o => o.CompanyId)
            .IsRequired();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.UserId)
            .IsRequired();

        builder.Property(o => o.VisitId);

        builder.Property(o => o.WarehouseId);

        builder.Property(o => o.OrderType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasIndex(o => o.CompanyId);
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasIndex(o => new { o.CompanyId, o.CreatedAt });

        builder.HasOne(o => o.Company)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Visit)
            .WithMany(v => v.Orders)
            .HasForeignKey(o => o.VisitId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Warehouse)
            .WithMany(w => w.Orders)
            .HasForeignKey(o => o.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
