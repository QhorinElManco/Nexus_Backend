using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexos.Domain.Entities.Customers;

namespace Nexos.Persistence.Configurations.Customers;

public class CustomerAssignmentConfiguration : BaseEntityConfiguration<CustomerAssignment>
{
    public override void Configure(EntityTypeBuilder<CustomerAssignment> builder)
    {
        base.Configure(builder);

        builder.ToTable("CustomerAssignments");

        builder.Property(ca => ca.CustomerId)
            .IsRequired();

        builder.Property(ca => ca.UserId)
            .IsRequired();

        builder.Property(ca => ca.DayOfWeek)
            .IsRequired();

        builder.Property(ca => ca.SequenceOrder)
            .IsRequired();

        builder.HasIndex(ca => new { ca.CustomerId, ca.UserId }).IsUnique();

        builder.HasOne(ca => ca.Customer)
            .WithMany(c => c.CustomerAssignments)
            .HasForeignKey(ca => ca.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ca => ca.User)
            .WithMany()
            .HasForeignKey(ca => ca.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
