using Nexus.Domain.Entities.Security;

namespace Nexus.Domain.Entities.Products;

public class Supplier : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Name { get; set; }
    public required string TaxId { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<SmartInventory> SmartInventories { get; set; } = [];
}
