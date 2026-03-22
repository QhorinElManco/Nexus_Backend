using Nexus.Domain.Entities.Security;

namespace Nexus.Domain.Entities.Products;

public class Category : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = [];
}
