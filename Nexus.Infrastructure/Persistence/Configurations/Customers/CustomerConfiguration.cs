using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus.Domain.Entities.Customers;

namespace Nexus.Infrastructure.Persistence.Configurations.Customers;

public class CustomerConfiguration : BaseEntityConfiguration<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> builder)
    {
        base.Configure(builder);

        builder.ToTable("Customers");

        builder.Property(c => c.CompanyId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.TradeName)
            .HasMaxLength(200);

        builder.Property(c => c.TaxId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Lat);

        builder.Property(c => c.Lng);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(c => c.CompanyId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => new { c.CompanyId, c.TaxId }).IsUnique();

        builder.HasOne(c => c.Company)
            .WithMany(co => co.Customers)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
