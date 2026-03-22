using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IProductService
{
    public Task<Response<ProductDto>> GetByIdAsync(long id, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<ProductDto>>> GetAllAsync(CancellationToken ct = default);
    public Task<Response<IReadOnlyList<ProductDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);
    public Task<Response<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    public Task<Response<ProductDto>> UpdateAsync(long id, UpdateProductDto dto, CancellationToken ct = default);
    public Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
}