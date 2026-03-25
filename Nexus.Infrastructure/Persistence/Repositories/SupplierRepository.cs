using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Products;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class SupplierRepository(NexusDbContext context) : ISupplierRepository
{
    public async Task<Supplier?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Suppliers
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Supplier> Items, int TotalCount)> SearchAsync(
        long companyId,
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.Suppliers
            .AsNoTracking()
            .Where(s => s.CompanyId == companyId && !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, $"%{term}%") ||
                EF.Functions.ILike(s.TaxId, $"%{term}%"));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Supplier> AddAsync(Supplier entity, CancellationToken ct = default)
    {
        context.Suppliers.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Supplier entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Suppliers.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Suppliers.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsByTaxIdAsync(long companyId, string taxId, CancellationToken ct = default)
    {
        return await context.Suppliers.AnyAsync(s => s.CompanyId == companyId && s.TaxId == taxId, ct);
    }
}
