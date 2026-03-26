using Microsoft.EntityFrameworkCore;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Domain.Entities.Products;

namespace Nexus.Infrastructure.Persistence.Repositories;

public class SmartInventoryRepository(NexusDbContext context) : ISmartInventoryRepository
{
    public async Task<SmartInventory?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await context.SmartInventories
            .AsNoTracking()
            .Include(si => si.Warehouse).ThenInclude(w => w.Company)
            .Include(si => si.Sku)
            .Include(si => si.Supplier)
            .FirstOrDefaultAsync(si => si.Id == id, ct);
    }

    public async Task<IReadOnlyList<SmartInventory>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.SmartInventories
            .AsNoTracking()
            .Include(si => si.Warehouse).ThenInclude(w => w.Company)
            .Include(si => si.Sku)
            .Include(si => si.Supplier)
            .OrderBy(si => si.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SmartInventory>> GetAllByCompanyAsync(long companyId,
        CancellationToken ct = default)
    {
        return await context.SmartInventories
            .AsNoTracking()
            .Include(si => si.Warehouse).ThenInclude(w => w.Company)
            .Include(si => si.Sku)
            .Include(si => si.Supplier)
            .Where(si => si.Warehouse.CompanyId == companyId)
            .OrderBy(si => si.Id)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<SmartInventory> Items, int TotalCount)> SearchAsync(
        long companyId,
        string? searchTerm,
        long? warehouseId,
        long? skuId,
        long? supplierId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.SmartInventories
            .AsNoTracking()
            .Include(si => si.Warehouse).ThenInclude(w => w.Company)
            .Include(si => si.Sku)
            .Include(si => si.Supplier)
            .Where(si => si.Warehouse.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(si =>
                EF.Functions.ILike(si.Warehouse.Name, $"%{term}%") ||
                EF.Functions.ILike(si.Sku.Barcode, $"%{term}%") ||
                EF.Functions.ILike(si.Supplier.Name, $"%{term}%"));
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(si => si.WarehouseId == warehouseId.Value);
        }

        if (skuId.HasValue)
        {
            query = query.Where(si => si.SkuId == skuId.Value);
        }

        if (supplierId.HasValue)
        {
            query = query.Where(si => si.SupplierId == supplierId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(si => si.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<bool> ExistsByWarehouseAndSkuAsync(long warehouseId, long skuId, long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = context.SmartInventories
            .Where(si => si.WarehouseId == warehouseId && si.SkuId == skuId);

        if (excludeId.HasValue)
        {
            query = query.Where(si => si.Id != excludeId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<SmartInventory> AddAsync(SmartInventory entity, CancellationToken ct = default)
    {
        context.SmartInventories.Add(entity);
        await context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(SmartInventory entity, CancellationToken ct = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.SmartInventories.Update(entity);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await context.SmartInventories.FindAsync([id], ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }
}
