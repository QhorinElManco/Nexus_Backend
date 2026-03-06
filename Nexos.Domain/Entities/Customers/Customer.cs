namespace Nexos.Domain.Entities.Customers;

public class Customer : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Name { get; set; }
    public string? TradeName { get; set; }
    public required string TaxId { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public required string Status { get; set; }

    public Security.Company Company { get; set; } = null!;
    public ICollection<CustomerAssignment> CustomerAssignments { get; set; } = [];
    public ICollection<Sales.Visit> Visits { get; set; } = [];
    public ICollection<Sales.Order> Orders { get; set; } = [];
}
