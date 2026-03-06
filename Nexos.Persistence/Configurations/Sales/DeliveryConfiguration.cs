using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexos.Domain.Entities.Sales;

namespace Nexos.Persistence.Configurations.Sales;

public class DeliveryConfiguration : BaseEntityConfiguration<Delivery>
{
    public override void Configure(EntityTypeBuilder<Delivery> builder)
    {
        base.Configure(builder);

        builder.ToTable("Deliveries");

        builder.Property(d => d.CompanyId)
            .IsRequired();

        builder.Property(d => d.OrderId)
            .IsRequired();

        builder.Property(d => d.UserId)
            .IsRequired();

        builder.Property(d => d.DeliveryTime);

        builder.Property(d => d.DeliveryLat);

        builder.Property(d => d.DeliveryLng);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.ProofOfDeliveryUrl)
            .HasMaxLength(500);

        builder.HasIndex(d => d.CompanyId);
        builder.HasIndex(d => d.OrderId);
        builder.HasIndex(d => new { d.CompanyId, d.CreatedAt });

        builder.HasOne(d => d.Company)
            .WithMany(c => c.Deliveries)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Order)
            .WithMany(o => o.Deliveries)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.User)
            .WithMany(u => u.Deliveries)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
