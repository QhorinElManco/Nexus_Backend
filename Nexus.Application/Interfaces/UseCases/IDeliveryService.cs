using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IDeliveryService
{
    public Task<Response<DeliveryDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<DeliveryDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<ResponsePagination<DeliveryDto>> SearchAsync(DeliverySearchRequest request, long companyId,
        CancellationToken ct = default);

    public Task<Response<DeliveryDto>> CreateAsync(CreateDeliveryDto dto, long companyId, long userId,
        CancellationToken ct = default);

    public Task<Response<DeliveryDto>> UpdateAsync(long id, UpdateDeliveryDto dto, long companyId,
        CancellationToken ct = default);
}
