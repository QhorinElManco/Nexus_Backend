using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IWarehouseTypeService
{
    public Task<Response<WarehouseTypeDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);

    public Task<Response<IReadOnlyList<WarehouseTypeDto>>> GetByCompanyAsync(long companyId,
        CancellationToken ct = default);

    public Task<Response<WarehouseTypeDto>> CreateAsync(CreateWarehouseTypeDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<WarehouseTypeDto>> UpdateAsync(long id, UpdateWarehouseTypeDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default);
}
