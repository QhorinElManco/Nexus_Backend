using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface ISkuService
{
    Task<Response<SkuDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Response<IReadOnlyList<SkuDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Response<IReadOnlyList<SkuDto>>> GetByProductAsync(long productId, CancellationToken ct = default);
    Task<Response<SkuDto>> CreateAsync(CreateSkuDto dto, CancellationToken ct = default);
    Task<Response<SkuDto>> UpdateAsync(long id, UpdateSkuDto dto, CancellationToken ct = default);
    Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
}