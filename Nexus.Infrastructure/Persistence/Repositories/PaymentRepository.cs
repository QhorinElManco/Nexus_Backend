using Microsoft.EntityFrameworkCore;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class PaymentRepository(NexusDbContext context) : IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.Payments
            .AsNoTracking()
            .Include(p => p.Order)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Payment>> GetByOrderIdAsync(long orderId, CancellationToken ct = default)
    {
        return await context.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CollectedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Payment>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.Payments
            .AsNoTracking()
            .Where(p => p.CompanyId == companyId)
            .OrderByDescending(p => p.CollectedAt)
            .ToListAsync(ct);
    }

    public async Task<Payment> AddAsync(Payment entity, CancellationToken ct = default)
    {
        context.Payments.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Payment entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Payments.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.Payments.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<(IReadOnlyList<Payment> Items, int TotalCount)> SearchAsync(
        PaymentSearchRequest request,
        long companyId,
        CancellationToken ct = default)
    {
        var query = context.Payments
            .AsNoTracking()
            .Include(p => p.Order)
            .Include(p => p.User)
            .Where(p => p.CompanyId == companyId);

        if (request.OrderId.HasValue)
        {
            query = query.Where(p => p.OrderId == request.OrderId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            query = query.Where(p => p.PaymentMethod == request.PaymentMethod);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(p => p.CollectedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(p => p.CollectedAt <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CollectedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<decimal> GetTotalPaymentsByOrderAsync(long orderId, CancellationToken ct = default)
    {
        return await context.Payments
            .Where(p => p.OrderId == orderId && !p.IsDeleted)
            .SumAsync(p => p.Amount, ct);
    }
}
