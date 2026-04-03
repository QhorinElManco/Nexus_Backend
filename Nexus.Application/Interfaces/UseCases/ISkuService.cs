using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface ISkuService
{
    public Task<Response<SkuDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);

    public Task<Response<IReadOnlyList<SkuDto>>> GetByProductAsync(long productId, long companyId,
        CancellationToken ct = default);

    public Task<Response<SkuDto>> CreateAsync(CreateSkuDto dto, long companyId, CancellationToken ct = default);

    public Task<Response<SkuDto>> UpdateAsync(long id, UpdateSkuDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default);
}
