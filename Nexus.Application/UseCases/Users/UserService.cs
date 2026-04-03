using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Response;
using Nexus.Application.Dto.Users;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Security;

namespace Nexus.Application.UseCases.Users;

public class UserService(
    IUserRepository userRepository,
    ICompanyRepository companyRepository,
    IRoleRepository roleRepository,
    IUserRoleRepository userRoleRepository,
    IPasswordHasher passwordHasher,
    IValidator<CreateUserDto> createValidator,
    IValidator<UpdateUserDto> updateValidator,
    IValidator<UserSearchRequest> searchValidator,
    ILogger<UserService> logger) : IUserService
{
    public async Task<Response<UserDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdWithRolesAsync(id, ct);

        if (user is null || user.CompanyId != companyId)
        {
            return Response<UserDto>.Fail("User not found", ErrorCode.NotFound);
        }

        return Response<UserDto>.Ok(MapToDto(user));
    }

    public async Task<Response<IReadOnlyList<UserDto>>> GetByCompanyAsync(long companyId,
        CancellationToken ct = default)
    {
        var users = await userRepository.GetByCompanyAsync(companyId, ct);
        return Response<IReadOnlyList<UserDto>>.Ok(users.Select(MapToDto).ToList());
    }

    public async Task<ResponsePagination<UserDto>> SearchAsync(UserSearchRequest request, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await searchValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponsePagination<UserDto>();
        }

        var (items, totalCount) = await userRepository.SearchAsync(
            request.SearchTerm,
            companyId,
            request.Page,
            request.PageSize,
            ct);

        return ResponsePagination<UserDto>.Ok(
            items.Select(MapToDto).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    public async Task<Response<UserDto>> CreateAsync(CreateUserDto dto, long companyId, CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<UserDto>();
        }

        var companyExists = await companyRepository.GetByIdAsync(companyId, ct);
        if (companyExists is null)
        {
            logger.LogWarning("Create user failed: company not found [{CompanyId}]", companyId);
            return Response<UserDto>.Fail("Company not found", ErrorCode.NotFound);
        }

        if (await userRepository.ExistsByUsernameAsync(dto.Username, ct: ct))
        {
            logger.LogWarning("Create user failed: username already exists [{Username}]", dto.Username);
            return Response<UserDto>.Fail("Username already exists", ErrorCode.Conflict);
        }

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = passwordHasher.Hash(dto.Password),
            FullName = dto.FullName,
            CompanyId = companyId,
            IsActive = true
        };

        var created = await userRepository.AddAsync(user, ct);

        logger.LogInformation("User created successfully [{UserId}] [{Username}] [{CompanyId}]", created.Id,
            created.Username, created.CompanyId);

        var userWithRelations = await userRepository.GetByIdWithRolesAsync(created.Id, ct);
        return Response<UserDto>.Ok(MapToDto(userWithRelations!));
    }

    public async Task<Response<UserDto>> UpdateAsync(long id, UpdateUserDto dto, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<UserDto>();
        }

        var user = await userRepository.GetByIdAsync(id, ct);
        if (user is null || user.CompanyId != companyId)
        {
            return Response<UserDto>.Fail("User not found", ErrorCode.NotFound);
        }

        user.FullName = dto.FullName;
        user.IsActive = dto.IsActive;

        await userRepository.UpdateAsync(user, ct);

        var userWithRelations = await userRepository.GetByIdWithRolesAsync(id, ct);
        return Response<UserDto>.Ok(MapToDto(userWithRelations!));
    }

    public async Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(id, ct);
        if (user is null || user.CompanyId != companyId)
        {
            logger.LogWarning("Delete user failed: user not found [{UserId}]", id);
            return Response<bool>.Fail("User not found", ErrorCode.NotFound);
        }

        await userRepository.DeleteAsync(id, ct);

        logger.LogInformation("User deleted (soft-delete) [{UserId}] [{Username}]", id, user.Username);
        return Response<bool>.Ok(true);
    }

    public async Task<Response<UserDto>> GetByUsernameAsync(string username, long companyId,
        CancellationToken ct = default)
    {
        var user = await userRepository.GetByUsernameWithRolesAsync(username, ct);

        if (user is null || user.CompanyId != companyId)
        {
            return Response<UserDto>.Fail("User not found", ErrorCode.NotFound);
        }

        return Response<UserDto>.Ok(MapToDto(user));
    }

    public async Task<Response<UserDto>> AssignRoleAsync(long userId, AssignRoleDto dto, long companyId,
        CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null || user.CompanyId != companyId)
        {
            logger.LogWarning("Assign role failed: user not found [{UserId}]", userId);
            return Response<UserDto>.Fail("User not found", ErrorCode.NotFound);
        }

        var role = await roleRepository.GetByIdAsync(dto.RoleId, ct);
        if (role is null || role.CompanyId != companyId)
        {
            logger.LogWarning("Assign role failed: role not found [{RoleId}]", dto.RoleId);
            return Response<UserDto>.Fail("Role not found", ErrorCode.NotFound);
        }

        if (await userRoleRepository.ExistsAsync(userId, dto.RoleId, ct))
        {
            logger.LogWarning("Assign role failed: user already has role [{UserId}] [{RoleId}]", userId, dto.RoleId);
            return Response<UserDto>.Fail("User already has this role", ErrorCode.Conflict);
        }

        var userRole = new UserRole { UserId = userId, RoleId = dto.RoleId };

        await userRoleRepository.AddAsync(userRole, ct);

        logger.LogInformation("Role assigned to user [{UserId}] [{RoleId}] [{RoleName}]", userId, dto.RoleId,
            role.Name);

        var userWithRelations = await userRepository.GetByIdWithRolesAsync(userId, ct);
        return Response<UserDto>.Ok(MapToDto(userWithRelations!));
    }

    public async Task<Response<bool>> RemoveRoleAsync(long userId, long roleId, long companyId,
        CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null || user.CompanyId != companyId)
        {
            logger.LogWarning("Remove role failed: user not found [{UserId}]", userId);
            return Response<bool>.Fail("User not found", ErrorCode.NotFound);
        }

        var userRole = await userRoleRepository.GetAsync(userId, roleId, ct);
        if (userRole is null)
        {
            logger.LogWarning("Remove role failed: assignment not found [{UserId}] [{RoleId}]", userId, roleId);
            return Response<bool>.Fail("User role assignment not found", ErrorCode.NotFound);
        }

        await userRoleRepository.RemoveAsync(userId, roleId, ct);

        logger.LogInformation("Role removed from user [{UserId}] [{RoleId}]", userId, roleId);
        return Response<bool>.Ok(true);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.FullName,
            user.CompanyId ?? 0, // 0 for superadmin
            user.Company?.Name ?? (user.CompanyId == null ? "SuperAdmin" : string.Empty),
            user.IsActive,
            user.UserRoles.Select(ur => new RoleDto(ur.Role.Id, ur.Role.Name, ur.Role.Description)).ToList(),
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}
