using Nexus.Application.Dto.Customers;
using Nexus.Domain.Entities.Customers;

namespace Nexus.Application.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    public Task<Customer?> GetByTaxIdAsync(string taxId, long companyId, CancellationToken ct = default);
    public Task<Customer?> GetByIdWithAssignmentsAsync(long id, CancellationToken ct = default);
    public Task<IReadOnlyList<Customer>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<bool> ExistsByTaxIdAsync(string taxId, long companyId, long? excludeId = null,
        CancellationToken ct = default);

    public Task<IReadOnlyList<CustomerAssignment>> GetAssignmentsByUserAsync(long userId, int? dayOfWeek = null,
        CancellationToken ct = default);

    public Task<(IReadOnlyList<Customer> Items, int TotalCount)> SearchAsync(
        CustomerSearchRequest request,
        CancellationToken ct = default);
}
