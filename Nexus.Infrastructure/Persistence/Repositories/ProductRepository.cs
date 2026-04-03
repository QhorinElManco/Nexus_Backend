using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Products;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class ProductRepository(NexusDbContext context) : IProductRepository
{
    public Task<Product?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<Product> AddAsync(Product entity, CancellationToken ct = default)
    {
        context.Products.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Product entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Products.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Products.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<Product>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.Products
            .AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = context.Products.Where(p => p.CompanyId == companyId && p.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public Task<Product?> GetByIdWithCategoryAsync(long id, CancellationToken ct = default)
    {
        return context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllWithCategoryAsync(CancellationToken ct = default)
    {
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByCompanyWithCategoryAsync(long companyId,
        CancellationToken ct = default)
    {
        return await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }
}
