using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexos.Domain.Entities.Products;

namespace Nexos.Persistence.Configurations.Products;

public class SupplierConfiguration : BaseEntityConfiguration<Supplier>
{
    public override void Configure(EntityTypeBuilder<Supplier> builder)
    {
        base.Configure(builder);

        builder.ToTable("Suppliers");

        builder.Property(s => s.CompanyId)
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.TaxId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.CompanyId);
        builder.HasIndex(s => new { s.CompanyId, s.TaxId }).IsUnique();

        builder.HasOne(s => s.Company)
            .WithMany(c => c.Suppliers)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
