using Nexus.Domain.Entities.Products;

namespace Nexus.Application.Interfaces.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    public Task<IReadOnlyList<Warehouse>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null,
        CancellationToken ct = default);
}
