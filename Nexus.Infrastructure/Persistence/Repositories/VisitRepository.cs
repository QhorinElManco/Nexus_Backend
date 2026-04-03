using Microsoft.EntityFrameworkCore;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class VisitRepository(NexusDbContext context) : IVisitRepository
{
    public async Task<Visit?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Visits
            .AsNoTracking()
            .Include(v => v.Customer)
            .Include(v => v.User)
            .FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<IReadOnlyList<Visit>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Visits
            .AsNoTracking()
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Visit>> GetByCustomerIdAsync(long customerId, CancellationToken ct = default)
    {
        return await context.Visits
            .AsNoTracking()
            .Include(v => v.User)
            .Where(v => v.CustomerId == customerId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Visit>> GetByUserIdAsync(long userId, CancellationToken ct = default)
    {
        return await context.Visits
            .AsNoTracking()
            .Include(v => v.Customer)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Visit>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.Visits
            .AsNoTracking()
            .Where(v => v.CompanyId == companyId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Visit> AddAsync(Visit entity, CancellationToken ct = default)
    {
        context.Visits.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Visit entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Visits.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Visits.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<(IReadOnlyList<Visit> Items, int TotalCount)> SearchAsync(
        VisitSearchRequest request,
        long companyId,
        CancellationToken ct = default)
    {
        var query = context.Visits
            .AsNoTracking()
            .Include(v => v.Customer)
            .Include(v => v.User)
            .Where(v => v.CompanyId == companyId);

        if (request.CustomerId.HasValue)
        {
            query = query.Where(v => v.CustomerId == request.CustomerId.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(v => v.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(v => v.Status == request.Status);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(v => v.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(v => v.CreatedAt <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
