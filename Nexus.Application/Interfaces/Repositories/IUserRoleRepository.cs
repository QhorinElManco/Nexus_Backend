using Nexus.Domain.Entities.Security;

namespace Nexus.Application.Interfaces.Repositories;

public interface IUserRoleRepository
{
    public Task<bool> ExistsAsync(long userId, long roleId, CancellationToken ct = default);
    public Task AddAsync(UserRole userRole, CancellationToken ct = default);
    public Task<UserRole?> GetAsync(long userId, long roleId, CancellationToken ct = default);
    public Task RemoveAsync(long userId, long roleId, CancellationToken ct = default);
}
