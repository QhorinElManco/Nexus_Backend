using Nexus.Application.Dto.Response;
using Nexus.Application.Dto.Users;

namespace Nexus.Application.Interfaces.UseCases;

public interface IUserService
{
    public Task<Response<UserDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<IReadOnlyList<UserDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);

    public Task<ResponsePagination<UserDto>> SearchAsync(UserSearchRequest request, long companyId,
        CancellationToken ct = default);

    public Task<Response<UserDto>> CreateAsync(CreateUserDto dto, long companyId, CancellationToken ct = default);

    public Task<Response<UserDto>> UpdateAsync(long id, UpdateUserDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default);
    public Task<Response<UserDto>> GetByUsernameAsync(string username, long companyId, CancellationToken ct = default);

    public Task<Response<UserDto>> AssignRoleAsync(long userId, AssignRoleDto dto, long companyId,
        CancellationToken ct = default);

    public Task<Response<bool>> RemoveRoleAsync(long userId, long roleId, long companyId,
        CancellationToken ct = default);
}
