using Nexus.Application.Dto.Companies;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface ICompanyService
{
    public Task<Response<CompanyDto>> GetByIdAsync(long id, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<CompanyDto>>> GetAllAsync(CancellationToken ct = default);

    public Task<ResponsePagination<CompanyDto>> SearchAsync(CompanySearchRequest request,
        CancellationToken ct = default);

    public Task<Response<CompanyDto>> CreateAsync(CreateCompanyDto dto, CancellationToken ct = default);

    public Task<Response<CompanyDto>> UpdateAsync(long id, UpdateCompanyDto dto,
        CancellationToken ct = default);

    public Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
