using Microsoft.EntityFrameworkCore;
using Nexos.Application.Interfaces.Repositories;
using Nexos.Domain.Entity.Security;

namespace Nexos.Persistence.Repositories;

public class CompanyRepository(NexosDbContext context) : ICompanyRepository
{
    public async Task<Company?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Companies
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Company> Items, int TotalCount)> SearchAsync(
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.Companies.AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(c =>
                c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.TaxId.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Company> AddAsync(Company entity, CancellationToken ct = default)
    {
        context.Companies.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Company entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Companies.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Companies.FindAsync(new object[] { id }, ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsByTaxIdAsync(string taxId, CancellationToken ct = default)
    {
        return await context.Companies.AnyAsync(c => c.TaxId == taxId && !c.IsDeleted, ct);
    }
}
