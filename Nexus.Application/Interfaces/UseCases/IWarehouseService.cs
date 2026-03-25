using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IWarehouseService
{
    public Task<Response<WarehouseDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<WarehouseDto>>> GetAllAsync(long companyId, CancellationToken ct = default);
    public Task<Response<WarehouseDto>> CreateAsync(CreateWarehouseDto dto, CancellationToken ct = default);
    public Task<Response<WarehouseDto>> UpdateAsync(long id, UpdateWarehouseDto dto, CancellationToken ct = default);
    public Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
}