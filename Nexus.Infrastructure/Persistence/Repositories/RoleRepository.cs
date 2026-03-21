using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Security;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class RoleRepository(NexusDbContext context) : IRoleRepository
{
    public Task<Role?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(long id, CancellationToken ct = default)
    {
        return await context.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Role>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.Roles
            .AsNoTracking()
            .Where(r => r.CompanyId == companyId)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<Role> AddAsync(Role entity, CancellationToken ct = default)
    {
        context.Roles.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Role entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Roles.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Roles.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, long companyId, long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = context.Roles.Where(r => r.Name == name && r.CompanyId == companyId);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }
}
