using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexos.Domain.Entities.Products;

namespace Nexos.Persistence.Configurations.Products;

public class ProductConfiguration : BaseEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.ToTable("Products");

        builder.Property(p => p.CompanyId)
            .IsRequired();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Category)
            .HasMaxLength(100);

        builder.Property(p => p.Brand)
            .HasMaxLength(100);

        builder.HasIndex(p => p.CompanyId);
        builder.HasIndex(p => p.Category);

        builder.HasOne(p => p.Company)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
