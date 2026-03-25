using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Products;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class WarehouseRepository(NexusDbContext context) : IWarehouseRepository
{
    public async Task<Warehouse?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Warehouses
            .AsNoTracking()
            .Include(w => w.Company)
            .Include(w => w.Manager)
            .Include(w => w.WarehouseType)
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<Warehouse>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.Warehouses
            .AsNoTracking()
            .Include(w => w.Company)
            .Include(w => w.Manager)
            .Include(w => w.WarehouseType)
            .Where(w => w.CompanyId == companyId && !w.IsDeleted)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = context.Warehouses.Where(w => w.CompanyId == companyId && w.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(w => w.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<Warehouse> AddAsync(Warehouse entity, CancellationToken ct = default)
    {
        context.Warehouses.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Warehouse entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Warehouses.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Warehouses.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Warehouses
            .AsNoTracking()
            .Include(w => w.Company)
            .Include(w => w.Manager)
            .Include(w => w.WarehouseType)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);
    }
}
