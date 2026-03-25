using Nexus.Domain.Entities.Products;

namespace Nexus.Application.Interfaces.Repositories;

public interface IWarehouseTypeRepository : IRepository<WarehouseType>
{
    public Task<IReadOnlyList<WarehouseType>> GetByCompanyAsync(long companyId, CancellationToken ct = default);
    public Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null, CancellationToken ct = default);
    public Task<bool> HasWarehousesAsync(long warehouseTypeId, CancellationToken ct = default);
}
