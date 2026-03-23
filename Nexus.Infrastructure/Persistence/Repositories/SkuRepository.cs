using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Products;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class SkuRepository(NexusDbContext context) : ISkuRepository
{
    public Task<Sku?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return context.Skus
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IReadOnlyList<Sku>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Skus
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Barcode)
            .ToListAsync(ct);
    }

    public async Task<Sku> AddAsync(Sku entity, CancellationToken ct = default)
    {
        context.Skus.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Sku entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Skus.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Skus.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<Sku>> GetByProductAsync(long productId, CancellationToken ct = default)
    {
        return await context.Skus
            .AsNoTracking()
            .Include(s => s.Product)
            .Where(s => s.ProductId == productId && s.IsActive)
            .OrderBy(s => s.Barcode)
            .ToListAsync(ct);
    }

    public Task<Sku?> GetByIdWithProductAsync(long id, CancellationToken ct = default)
    {
        return context.Skus
            .AsNoTracking()
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive, ct);
    }
}