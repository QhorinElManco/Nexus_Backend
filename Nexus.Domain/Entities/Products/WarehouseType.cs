using Nexus.Domain.Entities.Security;

namespace Nexus.Domain.Entities.Products;

public class WarehouseType : BaseEntity
{
    public required long CompanyId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public Company Company { get; set; } = null!;
    public ICollection<Warehouse> Warehouses { get; set; } = [];
}
