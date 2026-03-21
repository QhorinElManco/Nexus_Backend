using Nexus.Domain.Entities.Security;

namespace Nexus.Application.Interfaces.Repositories;

public interface IAccessRepository : IRepository<Access>
{
    Task<Access?> GetByIdWithRolesAsync(long id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, long? excludeId = null, CancellationToken ct = default);
}
