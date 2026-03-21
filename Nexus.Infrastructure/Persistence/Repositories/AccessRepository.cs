using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Security;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class AccessRepository(NexusDbContext context) : IAccessRepository
{
    public async Task<Access?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<Access?> GetByIdWithRolesAsync(long id, CancellationToken ct = default)
    {
        return await context.Permissions
            .AsNoTracking()
            .Include(a => a.RolePermissions)
                .ThenInclude(rp => rp.Role)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<IReadOnlyList<Access>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Permissions
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync(ct);
    }

    public async Task<Access> AddAsync(Access entity, CancellationToken ct = default)
    {
        context.Permissions.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Access entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Permissions.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Permissions.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, long? excludeId = null, CancellationToken ct = default)
    {
        var query = context.Permissions.Where(a => a.Name == name);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }
}
