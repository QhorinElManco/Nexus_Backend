namespace Nexos.Domain.Entities.Products;

public class Supplier : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Name { get; set; }
    public required string TaxId { get; set; }

    public Security.Company Company { get; set; } = null!;
    public ICollection<SmartInventory> SmartInventories { get; set; } = [];
}
