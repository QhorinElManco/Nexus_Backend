using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Infrastructure.Persistence.Configurations.Sales;

public class VisitConfiguration : BaseEntityConfiguration<Visit>
{
    public override void Configure(EntityTypeBuilder<Visit> builder)
    {
        base.Configure(builder);

        builder.ToTable("Visits");

        builder.Property(v => v.CompanyId)
            .IsRequired();

        builder.Property(v => v.UserId)
            .IsRequired();

        builder.Property(v => v.CustomerId)
            .IsRequired();

        builder.Property(v => v.CheckInTime);

        builder.Property(v => v.CheckOutTime);

        builder.Property(v => v.CheckInLat);

        builder.Property(v => v.CheckInLng);

        builder.Property(v => v.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.CancelReason)
            .HasMaxLength(500);

        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.CustomerId);
        builder.HasIndex(v => v.CreatedAt);
        builder.HasIndex(v => new { v.CompanyId, v.CreatedAt });

        builder.HasOne(v => v.Company)
            .WithMany(c => c.Visits)
            .HasForeignKey(v => v.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.User)
            .WithMany(u => u.Visits)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Customer)
            .WithMany(c => c.Visits)
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
