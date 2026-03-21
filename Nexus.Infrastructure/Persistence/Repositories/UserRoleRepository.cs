using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Security;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class UserRoleRepository(NexusDbContext context) : IUserRoleRepository
{
    public async Task<bool> ExistsAsync(long userId, long roleId, CancellationToken ct = default)
    {
        return await context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);
    }

    public async Task AddAsync(UserRole userRole, CancellationToken ct = default)
    {
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync(ct);
    }

    public async Task<UserRole?> GetAsync(long userId, long roleId, CancellationToken ct = default)
    {
        return await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);
    }

    public async Task RemoveAsync(long userId, long roleId, CancellationToken ct = default)
    {
        var userRole =
            await context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);
        if (userRole is not null)
        {
            context.UserRoles.Remove(userRole);
            await context.SaveChangesAsync(ct);
        }
    }
}
