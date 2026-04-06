namespace Nexus.Application.Dto.Products;

public record KardexEntryDto(
    long Id,
    long CompanyId,
    long WarehouseId,
    string? WarehouseName,
    long SkuId,
    string? SkuName,
    long UserId,
    string? UserName,
    string TransactionType,
    int Quantity,
    string? ReferenceDocType,
    string? ReferenceDocId,
    int StockBefore,
    int StockAfter,
    string? DeviceId,
    double? Lat,
    double? Lng,
    DateTime CreatedAt
);

public record ReconciliationResultDto(
    bool HasDiscrepancies,
    IReadOnlyList<DiscrepancyDto> Discrepancies,
    int CorrectedCount
);

public record DiscrepancyDto(
    long SmartInventoryId,
    long WarehouseId,
    long SkuId,
    int StoredQuantity,
    int CalculatedQuantity,
    int Difference,
    bool Corrected
);

public record ReconciliationRequestDto(
    bool Correct = false
);
