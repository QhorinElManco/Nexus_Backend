using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Products;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class CategoryRepository(NexusDbContext context) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Category> AddAsync(Category entity, CancellationToken ct = default)
    {
        context.Categories.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Category entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Categories.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Categories.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<Category>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.Categories
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = context.Categories.Where(c => c.CompanyId == companyId && c.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }
}
