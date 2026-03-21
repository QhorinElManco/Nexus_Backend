using Nexus.Domain.Entities.Security;

namespace Nexus.Application.Interfaces.Repositories;

public interface ICompanyRepository : IRepository<Company>
{
    public Task<bool> ExistsByTaxIdAsync(string taxId, CancellationToken ct = default);

    public Task<(IReadOnlyList<Company> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
