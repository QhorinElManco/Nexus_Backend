using Nexos.Domain.Entity.Security;

namespace Nexos.Domain.Entity.Products;

public class Supplier : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Name { get; set; }
    public required string TaxId { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<SmartInventory> SmartInventories { get; set; } = [];
}
