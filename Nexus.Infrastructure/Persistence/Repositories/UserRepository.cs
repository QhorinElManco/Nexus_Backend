using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Security;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class UserRepository(NexusDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);
    }

    public async Task<User> AddAsync(User entity, CancellationToken ct = default)
    {
        context.Users.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(User entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Users.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Users.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<User?> GetByUsernameWithRolesAsync(string username, CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<IReadOnlyList<User>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.CompanyId == companyId)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);
    }

    public async Task<User?> GetByIdWithRolesAsync(long id, CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = context.Users.Where(u => u.Username == username);

        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<User?> GetActiveUserByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, ct);
    }

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        long? companyId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(u =>
                EF.Functions.ILike(u.Username, $"%{term}%") ||
                EF.Functions.ILike(u.FullName, $"%{term}%"));
        }

        if (companyId.HasValue)
            query = query.Where(u => u.CompanyId == companyId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(u => u.Company)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
