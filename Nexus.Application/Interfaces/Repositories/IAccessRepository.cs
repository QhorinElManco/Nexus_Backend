using Nexus.Domain.Entities.Security;

namespace Nexus.Application.Interfaces.Repositories;

public interface IAccessRepository : IRepository<Access>
{
    public Task<Access?> GetByIdWithRolesAsync(long id, CancellationToken ct = default);
    public Task<bool> ExistsByNameAsync(string name, long? excludeId = null, CancellationToken ct = default);
}
