using Nexus.Application.Dto.Response;
using Nexus.Application.Dto.Users;

namespace Nexus.Application.Interfaces.UseCases;

public interface IUserService
{
    Task<Response<UserDto>> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Response<IReadOnlyList<UserDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ResponsePagination<UserDto>> SearchAsync(UserSearchRequest request, CancellationToken ct = default);
    Task<Response<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<Response<UserDto>> UpdateAsync(long id, UpdateUserDto dto, CancellationToken ct = default);
    Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default);
    Task<Response<UserDto>> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<Response<IReadOnlyList<UserDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default);
    Task<Response<UserDto>> AssignRoleAsync(long userId, AssignRoleDto dto, CancellationToken ct = default);
    Task<Response<bool>> RemoveRoleAsync(long userId, long roleId, CancellationToken ct = default);
}
