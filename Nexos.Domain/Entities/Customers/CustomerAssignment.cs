namespace Nexos.Domain.Entities.Customers;

public class CustomerAssignment : BaseEntity
{
    public required long CustomerId { get; set; }
    public required long UserId { get; set; }
    public required int DayOfWeek { get; set; }
    public int SequenceOrder { get; set; }

    public Customer Customer { get; set; } = null!;
    public Security.User User { get; set; } = null!;
}
