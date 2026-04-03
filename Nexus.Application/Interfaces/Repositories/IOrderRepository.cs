using Nexus.Application.Dto.Sales;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Application.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    public Task<Order?> GetByIdWithDetailsAsync(long id, CancellationToken ct = default);
    public Task<IReadOnlyList<Order>> GetByCompanyAsync(long companyId, CancellationToken ct = default);
    public Task<bool> ExistsByIdAsync(long id, long companyId, CancellationToken ct = default);

    public Task<(IReadOnlyList<Order> Items, int TotalCount)> SearchAsync(OrderSearchRequest request, long companyId,
        CancellationToken ct = default);
}
