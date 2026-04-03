using Microsoft.EntityFrameworkCore;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class DeliveryRepository(NexusDbContext context) : IDeliveryRepository
{
    public async Task<Delivery?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Deliveries
            .AsNoTracking()
            .Include(d => d.Order)
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<IReadOnlyList<Delivery>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Deliveries
            .AsNoTracking()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Delivery>> GetByOrderIdAsync(long orderId, CancellationToken ct = default)
    {
        return await context.Deliveries
            .AsNoTracking()
            .Include(d => d.User)
            .Where(d => d.OrderId == orderId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Delivery>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.Deliveries
            .AsNoTracking()
            .Where(d => d.CompanyId == companyId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Delivery> AddAsync(Delivery entity, CancellationToken ct = default)
    {
        context.Deliveries.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Delivery entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Deliveries.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Deliveries.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<(IReadOnlyList<Delivery> Items, int TotalCount)> SearchAsync(
        DeliverySearchRequest request,
        long companyId,
        CancellationToken ct = default)
    {
        var query = context.Deliveries
            .AsNoTracking()
            .Include(d => d.Order)
            .Include(d => d.User)
            .Where(d => d.CompanyId == companyId);

        if (request.OrderId.HasValue)
        {
            query = query.Where(d => d.OrderId == request.OrderId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(d => d.Status == request.Status);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
