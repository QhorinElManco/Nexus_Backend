using Nexus.Application.Dto.Products;
using Nexus.Domain.Entities.Transactions;

namespace Nexus.Application.Interfaces.Repositories;

public interface IKardexEntryRepository : IRepository<KardexEntry>
{
    public Task<IReadOnlyList<KardexEntry>> GetByCompanyAsync(long companyId, CancellationToken ct = default);
    public Task<IReadOnlyList<KardexEntry>> GetByWarehouseAsync(long warehouseId, CancellationToken ct = default);

    public Task<(IReadOnlyList<KardexEntry> Items, int TotalCount)> SearchAsync(
        KardexEntrySearchRequest request, long companyId, CancellationToken ct = default);
}
