using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class OrderDetailRepository(NexusDbContext context) : IOrderDetailRepository
{
    public async Task<OrderDetail?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.OrderDetails
            .AsNoTracking()
            .Include(od => od.Sku)
            .FirstOrDefaultAsync(od => od.Id == id, ct);
    }

    public async Task<IReadOnlyList<OrderDetail>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.OrderDetails
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrderDetail>> GetByOrderIdAsync(long orderId, CancellationToken ct = default)
    {
        return await context.OrderDetails
            .AsNoTracking()
            .Include(od => od.Sku)
            .Where(od => od.OrderId == orderId)
            .ToListAsync(ct);
    }

    public async Task<OrderDetail> AddAsync(OrderDetail entity, CancellationToken ct = default)
    {
        context.OrderDetails.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(OrderDetail entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.OrderDetails.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateRangeAsync(IReadOnlyList<OrderDetail> details, CancellationToken ct = default)
    {
        foreach (var detail in details)
        {
            detail.UpdatedAt = DateTime.UtcNow;
        }

        context.OrderDetails.UpdateRange(details);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.OrderDetails.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }
}
