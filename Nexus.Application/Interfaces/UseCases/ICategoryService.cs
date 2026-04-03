using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface ICategoryService
{
    public Task<Response<CategoryDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<CategoryDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<Response<CategoryDto>> CreateAsync(CreateCategoryDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default);
}
