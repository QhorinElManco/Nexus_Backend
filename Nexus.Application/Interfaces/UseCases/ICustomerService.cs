using Nexus.Application.Dto.Customers;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface ICustomerService
{
    public Task<Response<CustomerDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<CustomerDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<ResponsePagination<CustomerDto>> SearchAsync(CustomerSearchRequest request,
        CancellationToken ct = default);

    public Task<Response<CustomerDto>> CreateAsync(CreateCustomerDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<CustomerDto>> UpdateAsync(long id, UpdateCustomerDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<CustomerDto>> GetByTaxIdAsync(string taxId, long companyId, CancellationToken ct = default);
}
