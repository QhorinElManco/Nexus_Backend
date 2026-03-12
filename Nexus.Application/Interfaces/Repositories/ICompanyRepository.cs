using Nexus.Domain.Entities.Security;

namespace Nexus.Application.Interfaces.Repositories;

public interface ICompanyRepository : IRepository<Company>, ISearchableRepository<Company>
{
    public Task<bool> ExistsByTaxIdAsync(string taxId, CancellationToken ct = default);
}
