using Microsoft.EntityFrameworkCore;
using Nexus.Application.Dto.Products;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Transactions;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class KardexEntryRepository(NexusDbContext context) : IKardexEntryRepository
{
    public async Task<KardexEntry?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.KardexEntries
            .AsNoTracking()
            .Include(k => k.Warehouse)
            .Include(k => k.Sku)
            .Include(k => k.User)
            .FirstOrDefaultAsync(k => k.Id == id, ct);
    }

    public async Task<IReadOnlyList<KardexEntry>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.KardexEntries
            .AsNoTracking()
            .Include(k => k.Warehouse)
            .Include(k => k.Sku)
            .Include(k => k.User)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<KardexEntry> AddAsync(KardexEntry entity, CancellationToken ct = default)
    {
        context.KardexEntries.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(KardexEntry entity, CancellationToken ct = default)
    {
        throw new NotSupportedException("KardexEntry is an immutable audit log and cannot be updated.");
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        throw new NotSupportedException("KardexEntry is an immutable audit log and cannot be deleted.");
    }

    public async Task<IReadOnlyList<KardexEntry>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        return await context.KardexEntries
            .AsNoTracking()
            .Include(k => k.Warehouse)
            .Include(k => k.Sku)
            .Include(k => k.User)
            .Where(k => k.CompanyId == companyId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<KardexEntry>> GetByWarehouseAsync(long warehouseId, CancellationToken ct = default)
    {
        return await context.KardexEntries
            .AsNoTracking()
            .Include(k => k.Warehouse)
            .Include(k => k.Sku)
            .Include(k => k.User)
            .Where(k => k.WarehouseId == warehouseId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<KardexEntry> Items, int TotalCount)> SearchAsync(
        KardexEntrySearchRequest request, long companyId, CancellationToken ct = default)
    {
        var query = context.KardexEntries
            .AsNoTracking()
            .Include(k => k.Warehouse)
            .Include(k => k.Sku)
            .Include(k => k.User)
            .Where(k => k.CompanyId == companyId);

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(k => k.WarehouseId == request.WarehouseId.Value);
        }

        if (request.SkuId.HasValue)
        {
            query = query.Where(k => k.SkuId == request.SkuId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.TransactionType))
        {
            query = query.Where(k => k.TransactionType == request.TransactionType);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(k => k.CreatedAt >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(k => k.CreatedAt <= request.DateTo.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(k => k.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
