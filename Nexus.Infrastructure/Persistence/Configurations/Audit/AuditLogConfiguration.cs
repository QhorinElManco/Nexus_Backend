using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Audit;

namespace Nexus.Infrastructure.Persistence.Configurations.Audit;

public class AuditLogConfiguration : BaseEntityConfiguration<AuditLog>
{
    public override void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        base.Configure(builder);

        builder.ToTable("AuditLogs");

        builder.Property(a => a.CompanyId)
            .IsRequired();

        builder.Property(a => a.UserId);

        builder.Property(a => a.ModuleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.OldData)
            .HasColumnType("jsonb");

        builder.Property(a => a.NewData)
            .HasColumnType("jsonb");

        builder.Property(a => a.RiskLevel)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.DeviceId)
            .HasMaxLength(100);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.Lat);

        builder.Property(a => a.Lng);

        builder.HasIndex(a => a.CompanyId);
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => new { a.CompanyId, a.CreatedAt });
        builder.HasIndex(a => a.UserId);

        builder.HasOne(a => a.Company)
            .WithMany(c => c.AuditLogs)
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
