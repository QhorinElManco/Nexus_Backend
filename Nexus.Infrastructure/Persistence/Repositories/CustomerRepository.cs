using Microsoft.EntityFrameworkCore;
using Nexus.Application.Dto.Customers;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Customers;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class CustomerRepository(NexusDbContext context) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Customer> AddAsync(Customer entity, CancellationToken ct = default)
    {
        context.Customers.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Customer entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Customers.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Customers.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<Customer?> GetByTaxIdAsync(string taxId, long companyId, CancellationToken ct = default)
    {
        return await context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TaxId == taxId && c.CompanyId == companyId, ct);
    }

    public async Task<Customer?> GetByIdWithAssignmentsAsync(long id, CancellationToken ct = default)
    {
        return await context.Customers
            .AsNoTracking()
            .Include(c => c.CustomerAssignments)
            .ThenInclude(ca => ca.User)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<Customer>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.Customers
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByTaxIdAsync(string taxId, long companyId, long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = context.Customers.Where(c => c.TaxId == taxId && c.CompanyId == companyId);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<IReadOnlyList<CustomerAssignment>> GetAssignmentsByUserAsync(long userId, int? dayOfWeek = null,
        CancellationToken ct = default)
    {
        var query = context.CustomerAssignments
            .AsNoTracking()
            .Include(ca => ca.Customer)
            .Where(ca => ca.UserId == userId);

        if (dayOfWeek.HasValue)
        {
            query = query.Where(ca => ca.DayOfWeek == dayOfWeek.Value);
        }

        return await query
            .OrderBy(ca => ca.SequenceOrder)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> SearchAsync(
        CustomerSearchRequest request,
        CancellationToken ct = default)
    {
        var query = context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(c =>
                EF.Functions.ILike(c.Name, $"%{term}%") ||
                (c.TradeName != null && EF.Functions.ILike(c.TradeName, $"%{term}%")) ||
                EF.Functions.ILike(c.TaxId, $"%{term}%"));
        }

        if (request.CompanyId.HasValue)
        {
            query = query.Where(c => c.CompanyId == request.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(c => c.Status == request.Status);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(c => c.Company)
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
