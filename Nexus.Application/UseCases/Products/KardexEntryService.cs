using System.Globalization;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Transactions;

namespace Nexus.Application.UseCases.Products;

public class KardexEntryService(
    IKardexEntryRepository kardexRepository,
    ISmartInventoryRepository inventoryRepository,
    ILogger<KardexEntryService> logger) : IKardexEntryService
{
    private static readonly HashSet<string> _validTransactionTypes =
        ["Sale", "Return", "Adjustment", "Purchase", "Transfer"];

    private static readonly HashSet<string> _inboundTypes = ["Return", "Purchase", "Adjustment"];
    private static readonly HashSet<string> _outboundTypes = ["Sale", "Transfer"];

    public async Task<Response<KardexEntryDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var entry = await kardexRepository.GetByIdAsync(id, ct);

        if (entry is null || entry.CompanyId != companyId)
        {
            logger.LogWarning("Kardex entry not found [{EntryId}] [{CompanyId}]", id, companyId);
            return Response<KardexEntryDto>.Fail("Kardex entry not found", ErrorCode.NotFound);
        }

        return Response<KardexEntryDto>.Ok(MapToDto(entry));
    }

    public async Task<Response<IReadOnlyList<KardexEntryDto>>> GetByCompanyAsync(long companyId,
        CancellationToken ct = default)
    {
        var entries = await kardexRepository.GetByCompanyAsync(companyId, ct);
        return Response<IReadOnlyList<KardexEntryDto>>.Ok(entries.Select(MapToDto).ToList());
    }

    public async Task<Response<IReadOnlyList<KardexEntryDto>>> GetByWarehouseAsync(long warehouseId, long companyId,
        CancellationToken ct = default)
    {
        var entries = await kardexRepository.GetByWarehouseAsync(warehouseId, ct);
        // Filter by company (warehouse-level query doesn't enforce company scope)
        var filtered = entries.Where(e => e.CompanyId == companyId).ToList();
        return Response<IReadOnlyList<KardexEntryDto>>.Ok(filtered.Select(MapToDto).ToList());
    }

    public async Task<ResponsePagination<KardexEntryDto>> SearchAsync(KardexEntrySearchRequest request, long companyId,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await kardexRepository.SearchAsync(request, companyId, ct);
        return ResponsePagination<KardexEntryDto>.Ok(
            items.Select(MapToDto).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    public async Task CreateEntryAsync(long companyId, long warehouseId, long skuId, long userId,
        string transactionType, int quantity, string referenceDocType, string referenceDocId,
        string? deviceId = null, double? lat = null, double? lng = null,
        CancellationToken ct = default)
    {
        if (!_validTransactionTypes.Contains(transactionType))
        {
            throw new ArgumentException(
                $"Invalid TransactionType. Must be one of: {string.Join(", ", _validTransactionTypes)}",
                nameof(transactionType));
        }

        var inventory = await inventoryRepository.GetStockAsync(warehouseId, skuId, ct);
        if (inventory is null)
        {
            throw new InvalidOperationException(
                $"No stock record found for SKU [{skuId}] in warehouse [{warehouseId}]");
        }

        var stockBefore = inventory.CurrentQuantity;
        int stockAfter;

        if (_inboundTypes.Contains(transactionType))
        {
            stockAfter = stockBefore + quantity;
            await inventoryRepository.UpdateStockAsync(warehouseId, skuId, quantity, ct);
        }
        else if (_outboundTypes.Contains(transactionType))
        {
            stockAfter = stockBefore - quantity;
            await inventoryRepository.UpdateStockAsync(warehouseId, skuId, -quantity, ct);
        }
        else
        {
            // Adjustment: quantity is the delta itself (positive or negative)
            stockAfter = stockBefore + quantity;
            await inventoryRepository.UpdateStockAsync(warehouseId, skuId, quantity, ct);
        }

        var entry = new KardexEntry
        {
            CompanyId = companyId,
            WarehouseId = warehouseId,
            SkuId = skuId,
            UserId = userId,
            TransactionType = transactionType,
            Quantity = Math.Abs(quantity),
            ReferenceDocType = referenceDocType,
            ReferenceDocId = referenceDocId,
            StockBefore = stockBefore,
            StockAfter = stockAfter,
            DeviceId = deviceId,
            Lat = lat,
            Lng = lng
        };

        await kardexRepository.AddAsync(entry, ct);

        logger.LogInformation(
            "Kardex entry created [{TransactionType}] [SKU:{SkuId}] [WH:{WarehouseId}] [Qty:{Quantity}] [Before:{StockBefore}] [After:{StockAfter}]",
            transactionType, skuId, warehouseId, quantity, stockBefore, stockAfter);
    }

    public async Task<Response<ReconciliationResultDto>> ReconcileAsync(long companyId, bool correct = false,
        CancellationToken ct = default)
    {
        var inventories = await inventoryRepository.GetAllByCompanyAsync(companyId, ct);
        var discrepancies = new List<DiscrepancyDto>();

        foreach (var inv in inventories)
        {
            var entries = await kardexRepository.GetByCompanyAsync(companyId, ct);
            var relevant = entries
                .Where(e => e.WarehouseId == inv.WarehouseId && e.SkuId == inv.SkuId)
                .OrderBy(e => e.CreatedAt)
                .ToList();

            var calculated = 0;
            foreach (var entry in relevant)
            {
                if (_inboundTypes.Contains(entry.TransactionType))
                {
                    calculated += entry.Quantity;
                }
                else if (_outboundTypes.Contains(entry.TransactionType))
                {
                    calculated -= entry.Quantity;
                }
                else if (entry.TransactionType == "Adjustment")
                {
                    // For Adjustment, use StockAfter - StockBefore to get the actual delta
                    calculated += entry.StockAfter - entry.StockBefore;
                }
            }

            if (calculated != inv.CurrentQuantity)
            {
                if (correct)
                {
                    var delta = calculated - inv.CurrentQuantity;
                    await inventoryRepository.UpdateStockAsync(inv.WarehouseId, inv.SkuId, delta, ct);

                    // Create an Adjustment KardexEntry documenting the correction
                    var correctionEntry = new KardexEntry
                    {
                        CompanyId = companyId,
                        WarehouseId = inv.WarehouseId,
                        SkuId = inv.SkuId,
                        UserId = inv.SupplierId, // Use SupplierId as placeholder; ideally use system user
                        TransactionType = "Adjustment",
                        Quantity = Math.Abs(delta),
                        ReferenceDocType = "Reconciliation",
                        ReferenceDocId = inv.Id.ToString(CultureInfo.InvariantCulture),
                        StockBefore = inv.CurrentQuantity,
                        StockAfter = calculated,
                        DeviceId = "system-reconciliation",
                        Lat = null,
                        Lng = null
                    };
                    await kardexRepository.AddAsync(correctionEntry, ct);
                }

                discrepancies.Add(new DiscrepancyDto(
                    inv.Id,
                    inv.WarehouseId,
                    inv.SkuId,
                    inv.CurrentQuantity,
                    calculated,
                    calculated - inv.CurrentQuantity,
                    correct
                ));
            }
        }

        var result = new ReconciliationResultDto(
            discrepancies.Count > 0,
            discrepancies,
            discrepancies.Count(d => d.Corrected)
        );

        logger.LogInformation("Reconciliation completed [{CompanyId}] [{Discrepancies}] [{Corrected}]",
            companyId, discrepancies.Count, result.CorrectedCount);

        return Response<ReconciliationResultDto>.Ok(result);
    }

    private static KardexEntryDto MapToDto(KardexEntry entry)
    {
        return new KardexEntryDto(
            entry.Id,
            entry.CompanyId,
            entry.WarehouseId,
            entry.Warehouse.Name,
            entry.SkuId,
            entry.Sku.Barcode,
            entry.UserId,
            entry.User.FullName,
            entry.TransactionType,
            entry.Quantity,
            entry.ReferenceDocType,
            entry.ReferenceDocId,
            entry.StockBefore,
            entry.StockAfter,
            entry.DeviceId,
            entry.Lat,
            entry.Lng,
            entry.CreatedAt
        );
    }
}
