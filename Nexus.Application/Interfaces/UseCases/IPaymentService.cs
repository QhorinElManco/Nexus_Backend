using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IPaymentService
{
    public Task<Response<PaymentDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);

    public Task<Response<IReadOnlyList<PaymentDto>>> GetByOrderIdAsync(long orderId, long companyId,
        CancellationToken ct = default);

    public Task<ResponsePagination<PaymentDto>> SearchAsync(PaymentSearchRequest request, long companyId,
        CancellationToken ct = default);

    public Task<Response<PaymentDto>> CreateAsync(CreatePaymentDto dto, long companyId, long userId,
        CancellationToken ct = default);
}
