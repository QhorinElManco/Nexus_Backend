using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IKardexEntryService
{
    public Task<Response<KardexEntryDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);

    public Task<Response<IReadOnlyList<KardexEntryDto>>> GetByCompanyAsync(long companyId,
        CancellationToken ct = default);

    public Task<Response<IReadOnlyList<KardexEntryDto>>> GetByWarehouseAsync(long warehouseId, long companyId,
        CancellationToken ct = default);

    public Task<ResponsePagination<KardexEntryDto>> SearchAsync(KardexEntrySearchRequest request, long companyId,
        CancellationToken ct = default);

    public Task CreateEntryAsync(long companyId, long warehouseId, long skuId, long userId,
        string transactionType, int quantity, string referenceDocType, string referenceDocId,
        string? deviceId = null, double? lat = null, double? lng = null,
        CancellationToken ct = default);

    public Task<Response<ReconciliationResultDto>> ReconcileAsync(long companyId, bool correct = false,
        CancellationToken ct = default);
}
