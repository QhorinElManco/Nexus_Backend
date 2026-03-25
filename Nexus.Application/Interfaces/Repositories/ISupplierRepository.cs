using Nexus.Domain.Entities.Products;

namespace Nexus.Application.Interfaces.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    public Task<bool> ExistsByTaxIdAsync(long companyId, string taxId, CancellationToken ct = default);

    public Task<(IReadOnlyList<Supplier> Items, int TotalCount)> SearchAsync(
        long companyId,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
