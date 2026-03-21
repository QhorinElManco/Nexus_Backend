using Nexus.Application.Dto.Access;
using Nexus.Application.Dto.Response;

namespace Nexus.Application.Interfaces.UseCases;

public interface IAccessService
{
    Task<Response<AccessDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Response<IReadOnlyList<AccessDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Response<AccessDto>> CreateAsync(CreateAccessDto dto, CancellationToken ct = default);
    Task<Response<AccessDto>> UpdateAsync(long id, UpdateAccessDto dto, CancellationToken ct = default);
    Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
