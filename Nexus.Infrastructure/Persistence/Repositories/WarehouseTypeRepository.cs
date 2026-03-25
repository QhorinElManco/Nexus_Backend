using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Products;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class WarehouseTypeRepository(NexusDbContext context) : IWarehouseTypeRepository
{
    public Task<WarehouseType?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return context.WarehouseTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(wt => wt.Id == id, ct);
    }

    public async Task<IReadOnlyList<WarehouseType>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.WarehouseTypes
            .AsNoTracking()
            .OrderBy(wt => wt.Name)
            .ToListAsync(ct);
    }

    public async Task<WarehouseType> AddAsync(WarehouseType entity, CancellationToken ct = default)
    {
        context.WarehouseTypes.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(WarehouseType entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.WarehouseTypes.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.WarehouseTypes.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<WarehouseType>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.WarehouseTypes
            .AsNoTracking()
            .Where(wt => wt.CompanyId == companyId)
            .OrderBy(wt => wt.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = context.WarehouseTypes.Where(wt => wt.CompanyId == companyId && wt.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(wt => wt.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<bool> HasWarehousesAsync(long warehouseTypeId, CancellationToken ct = default)
    {
        return await context.Warehouses
            .AnyAsync(w => w.WarehouseTypeId == warehouseTypeId && !w.IsDeleted, ct);
    }
}
