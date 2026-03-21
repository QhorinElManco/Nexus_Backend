using Nexus.Application.Dto.Access;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IAccessService
{
    public Task<Response<AccessDto>> GetByIdAsync(long id, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<AccessDto>>> GetAllAsync(CancellationToken ct = default);
    public Task<Response<AccessDto>> CreateAsync(CreateAccessDto dto, CancellationToken ct = default);
    public Task<Response<AccessDto>> UpdateAsync(long id, UpdateAccessDto dto, CancellationToken ct = default);
    public Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
