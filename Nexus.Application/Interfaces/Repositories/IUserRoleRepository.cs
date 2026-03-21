using Nexus.Domain.Entities.Security;

namespace Nexus.Application.Interfaces.Repositories;

public interface IUserRoleRepository
{
    Task<bool> ExistsAsync(long userId, long roleId, CancellationToken ct = default);
    Task AddAsync(UserRole userRole, CancellationToken ct = default);
    Task<UserRole?> GetAsync(long userId, long roleId, CancellationToken ct = default);
    Task RemoveAsync(long userId, long roleId, CancellationToken ct = default);
}
