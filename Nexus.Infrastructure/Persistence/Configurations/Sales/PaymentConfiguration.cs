using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Infrastructure.Persistence.Configurations.Sales;

public class PaymentConfiguration : BaseEntityConfiguration<Payment>
{
    public override void Configure(EntityTypeBuilder<Payment> builder)
    {
        base.Configure(builder);

        builder.ToTable("Payments");

        builder.Property(p => p.CompanyId)
            .IsRequired();

        builder.Property(p => p.OrderId)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.CollectedAt);

        builder.Property(p => p.Lat);

        builder.Property(p => p.Lng);

        builder.HasIndex(p => p.CompanyId);
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => new { p.CompanyId, p.CreatedAt });

        builder.HasOne(p => p.Company)
            .WithMany(c => c.Payments)
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
