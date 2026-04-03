using Microsoft.EntityFrameworkCore;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class OrderRepository(NexusDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(long id, CancellationToken ct = default)
    {
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Sku)
            .Include(o => o.Customer)
            .Include(o => o.User)
            .Include(o => o.Payments)
            .Include(o => o.Deliveries)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Orders
            .AsNoTracking()
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.Orders
            .AsNoTracking()
            .Where(o => o.CompanyId == companyId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        return await context.Orders
            .AsNoTracking()
            .AnyAsync(o => o.Id == id && o.CompanyId == companyId, ct);
    }

    public async Task<Order> AddAsync(Order entity, CancellationToken ct = default)
    {
        context.Orders.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Order entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Orders.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Orders.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> SearchAsync(
        OrderSearchRequest request,
        long companyId,
        CancellationToken ct = default)
    {
        var query = context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.User)
            .Where(o => o.CompanyId == companyId);

        if (request.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.OrderType))
        {
            query = query.Where(o => o.OrderType == request.OrderType);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(o => o.Status == request.Status);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == request.UserId.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
