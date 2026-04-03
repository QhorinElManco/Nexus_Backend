using Nexus.Application.Dto.Sales;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Application.Interfaces.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    public Task<IReadOnlyList<Payment>> GetByOrderIdAsync(long orderId, CancellationToken ct = default);
    public Task<IReadOnlyList<Payment>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<(IReadOnlyList<Payment> Items, int TotalCount)> SearchAsync(PaymentSearchRequest request,
        long companyId, CancellationToken ct = default);

    public Task<decimal> GetTotalPaymentsByOrderAsync(long orderId, CancellationToken ct = default);
}
