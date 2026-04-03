using Nexus.Application.Dto.Sales;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Application.Interfaces.Repositories;

public interface IDeliveryRepository : IRepository<Delivery>
{
    public new Task<Delivery?> GetByIdAsync(long id, CancellationToken ct = default);
    public Task<IReadOnlyList<Delivery>> GetByOrderIdAsync(long orderId, CancellationToken ct = default);
    public Task<IReadOnlyList<Delivery>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<(IReadOnlyList<Delivery> Items, int TotalCount)> SearchAsync(DeliverySearchRequest request,
        long companyId, CancellationToken ct = default);
}
