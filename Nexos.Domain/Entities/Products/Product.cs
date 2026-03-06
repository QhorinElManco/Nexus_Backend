namespace Nexos.Domain.Entities.Products;

public class Product : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Name { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }

    public Security.Company Company { get; set; } = null!;
    public ICollection<Sku> Skus { get; set; } = [];
}
