using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface ICategoryService
{
    public Task<Response<CategoryDto>> GetByIdAsync(long id, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<CategoryDto>>> GetAllAsync(CancellationToken ct = default);
    public Task<Response<IReadOnlyList<CategoryDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);
    public Task<Response<CategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
    public Task<Response<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto, CancellationToken ct = default);
    public Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
