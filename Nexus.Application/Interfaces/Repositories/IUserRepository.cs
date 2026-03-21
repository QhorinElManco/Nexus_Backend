using Nexus.Domain.Entities.Security;

namespace Nexus.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByUsernameWithRolesAsync(string username, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByCompanyAsync(long companyId, CancellationToken ct = default);
    Task<User?> GetByIdWithRolesAsync(long id, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, long? excludeId = null, CancellationToken ct = default);
    Task<User?> GetActiveUserByUsernameAsync(string username, CancellationToken ct = default);

    Task<(IReadOnlyList<User> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        long? companyId,
        int page,
        int pageSize,
        CancellationToken ct = default);
}
