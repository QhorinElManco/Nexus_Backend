using Nexos.Domain.Entity.Security;

namespace Nexos.Domain.Entity.Products;

public class Product : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Name { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<Sku> Skus { get; set; } = [];
}
