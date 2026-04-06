using Nexus.Domain.Entities.Products;

namespace Nexus.Application.Interfaces.Repositories;

public interface ISmartInventoryRepository : IRepository<SmartInventory>
{
    public Task<IReadOnlyList<SmartInventory>> GetAllByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<(IReadOnlyList<SmartInventory> Items, int TotalCount)> SearchAsync(
        long companyId,
        string? searchTerm,
        long? warehouseId,
        long? skuId,
        long? supplierId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    public Task<bool> ExistsByWarehouseAndSkuAsync(long warehouseId, long skuId, long? excludeId = null,
        CancellationToken ct = default);

    public Task<SmartInventory?> GetStockAsync(long warehouseId, long skuId, CancellationToken ct = default);
    public Task<int> UpdateStockAsync(long warehouseId, long skuId, int delta, CancellationToken ct = default);
}
