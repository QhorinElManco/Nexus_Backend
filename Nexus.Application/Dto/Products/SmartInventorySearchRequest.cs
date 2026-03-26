namespace Nexus.Application.Dto.Products;

public record SmartInventorySearchRequest(
    long CompanyId,
    string? SearchTerm,
    long? WarehouseId,
    long? SkuId,
    long? SupplierId,
    int Page = 1,
    int PageSize = 50
);
