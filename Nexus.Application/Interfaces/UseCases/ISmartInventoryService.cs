using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface ISmartInventoryService
{
    public Task<Response<SmartInventoryDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);

    public Task<Response<IReadOnlyList<SmartInventoryDto>>> GetAllAsync(long companyId, CancellationToken ct = default);

    public Task<ResponsePagination<SmartInventoryDto>> SearchAsync(SmartInventorySearchRequest request,
        CancellationToken ct = default);

    public Task<Response<SmartInventoryDto>> CreateAsync(CreateSmartInventoryDto dto, CancellationToken ct = default);

    public Task<Response<SmartInventoryDto>> UpdateAsync(long id, UpdateSmartInventoryDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default);
}
