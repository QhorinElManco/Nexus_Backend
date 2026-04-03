using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IOrderService
{
    public Task<Response<OrderDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<OrderDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<ResponsePagination<OrderDto>> SearchAsync(OrderSearchRequest request, long companyId,
        CancellationToken ct = default);

    public Task<Response<OrderDto>> CreateAsync(CreateOrderDto dto, long companyId, long userId,
        CancellationToken ct = default);

    public Task<Response<OrderDto>> UpdateAsync(long id, UpdateOrderDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default);

    public Task<Response<OrderDto>> AddDetailAsync(long orderId, CreateOrderDetailDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<bool>> RemoveDetailAsync(long orderId, long detailId, long companyId,
        CancellationToken ct = default);
}
