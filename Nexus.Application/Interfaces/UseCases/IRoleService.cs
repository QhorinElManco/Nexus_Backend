using Nexus.Application.Dto.Response;
using Nexus.Application.Dto.Roles;

namespace Nexus.Application.Interfaces.UseCases;

public interface IRoleService
{
    public Task<Response<RoleDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<RoleDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);
    public Task<Response<RoleDto>> CreateAsync(CreateRoleDto dto, long companyId, CancellationToken ct = default);

    public Task<Response<RoleDto>> UpdateAsync(long id, UpdateRoleDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default);
}
