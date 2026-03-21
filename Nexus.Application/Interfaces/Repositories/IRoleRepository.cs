using Nexus.Domain.Entities.Security;

namespace Nexus.Application.Interfaces.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByIdWithPermissionsAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetByCompanyAsync(long companyId, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, long companyId, long? excludeId = null, CancellationToken ct = default);
}
