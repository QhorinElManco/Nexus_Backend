using Nexus.Application.Dto.Response;
using Nexus.Application.Dto.Roles;

namespace Nexus.Application.Interfaces.UseCases;

public interface IRoleService
{
    public Task<Response<RoleDto>> GetByIdAsync(long id, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<RoleDto>>> GetAllAsync(CancellationToken ct = default);
    public Task<Response<IReadOnlyList<RoleDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);
    public Task<Response<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken ct = default);
    public Task<Response<RoleDto>> UpdateAsync(long id, UpdateRoleDto dto, CancellationToken ct = default);
    public Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
}
