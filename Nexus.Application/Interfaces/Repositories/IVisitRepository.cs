using Nexus.Application.Dto.Sales;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Application.Interfaces.Repositories;

public interface IVisitRepository : IRepository<Visit>
{
    public new Task<Visit?> GetByIdAsync(long id, CancellationToken ct = default);
    public Task<IReadOnlyList<Visit>> GetByCustomerIdAsync(long customerId, CancellationToken ct = default);
    public Task<IReadOnlyList<Visit>> GetByUserIdAsync(long userId, CancellationToken ct = default);
    public Task<IReadOnlyList<Visit>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<(IReadOnlyList<Visit> Items, int TotalCount)> SearchAsync(VisitSearchRequest request, long companyId,
        CancellationToken ct = default);
}
