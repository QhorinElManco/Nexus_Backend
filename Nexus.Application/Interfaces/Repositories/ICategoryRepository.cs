using Nexus.Domain.Entities.Products;

namespace Nexus.Application.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    public Task<IReadOnlyList<Category>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null,
        CancellationToken ct = default);
}
