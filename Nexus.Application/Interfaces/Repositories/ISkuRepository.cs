using Nexus.Domain.Entities.Products;

namespace Nexus.Application.Interfaces.Repositories;

public interface ISkuRepository : IRepository<Sku>
{
    public Task<IReadOnlyList<Sku>> GetByProductAsync(long productId, CancellationToken ct = default);
    public Task<Sku?> GetByIdWithProductAsync(long id, CancellationToken ct = default);
}