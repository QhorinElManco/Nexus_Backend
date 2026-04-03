using Nexus.Domain.Entities.Products;

namespace Nexus.Application.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    public Task<IReadOnlyList<Product>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null,
        CancellationToken ct = default);

    public Task<Product?> GetByIdWithCategoryAsync(long id, CancellationToken ct = default);
    public Task<IReadOnlyList<Product>> GetAllWithCategoryAsync(CancellationToken ct = default);
    public Task<IReadOnlyList<Product>> GetByCompanyWithCategoryAsync(long companyId, CancellationToken ct = default);
}
