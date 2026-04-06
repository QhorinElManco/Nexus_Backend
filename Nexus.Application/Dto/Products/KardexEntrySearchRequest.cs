namespace Nexus.Application.Dto.Products;

public record KardexEntrySearchRequest(
    int Page = 1,
    int PageSize = 20,
    long? WarehouseId = null,
    long? SkuId = null,
    string? TransactionType = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null
);
