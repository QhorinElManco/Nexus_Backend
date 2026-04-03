using Nexus.Domain.Entities.Sales;

namespace Nexus.Application.Interfaces.Repositories;

public interface IOrderDetailRepository : IRepository<OrderDetail>
{
    public new Task<OrderDetail?> GetByIdAsync(long id, CancellationToken ct = default);
    public Task<IReadOnlyList<OrderDetail>> GetByOrderIdAsync(long orderId, CancellationToken ct = default);
    public Task UpdateRangeAsync(IReadOnlyList<OrderDetail> details, CancellationToken ct = default);
}
