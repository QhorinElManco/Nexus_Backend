using Nexus.Application.Dto.Suppliers;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface ISupplierService
{
    public Task<Response<SupplierDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<SupplierDto>>> GetAllAsync(long companyId, CancellationToken ct = default);

    public Task<ResponsePagination<SupplierDto>> SearchAsync(SupplierSearchRequest request,
        CancellationToken ct = default);

    public Task<Response<SupplierDto>> CreateAsync(long companyId, CreateSupplierDto dto, CancellationToken ct = default);

    public Task<Response<SupplierDto>> UpdateAsync(long id, UpdateSupplierDto dto,
        CancellationToken ct = default);

    public Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
