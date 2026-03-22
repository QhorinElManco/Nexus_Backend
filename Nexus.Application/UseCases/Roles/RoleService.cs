using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Response;
using Nexus.Application.Dto.Roles;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Security;

namespace Nexus.Application.UseCases.Roles;

public class RoleService(
    IRoleRepository roleRepository,
    ICompanyRepository companyRepository,
    IValidator<CreateRoleDto> createValidator,
    IValidator<UpdateRoleDto> updateValidator,
    ILogger<RoleService> logger) : IRoleService
{
    public async Task<Response<RoleDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var role = await roleRepository.GetByIdWithPermissionsAsync(id, ct);

        return role is null
            ? Response<RoleDto>.Fail("Role not found", ErrorCode.NotFound)
            : Response<RoleDto>.Ok(MapToDto(role));
    }

    public async Task<Response<IReadOnlyList<RoleDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var roles = await roleRepository.GetAllAsync(ct);
        return Response<IReadOnlyList<RoleDto>>.Ok(roles.Select(MapToDto).ToList());
    }

    public async Task<Response<IReadOnlyList<RoleDto>>> GetByCompanyAsync(long companyId,
        CancellationToken ct = default)
    {
        var roles = await roleRepository.GetByCompanyAsync(companyId, ct);
        return Response<IReadOnlyList<RoleDto>>.Ok(roles.Select(MapToDto).ToList());
    }

    public async Task<Response<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<RoleDto>();
        }

        var companyExists = await companyRepository.GetByIdAsync(dto.CompanyId, ct);
        if (companyExists is null)
        {
            logger.LogWarning("Create role failed: company not found [{CompanyId}]", dto.CompanyId);
            return Response<RoleDto>.Fail("Company not found", ErrorCode.NotFound);
        }

        if (await roleRepository.ExistsByNameAsync(dto.Name, dto.CompanyId, ct: ct))
        {
            logger.LogWarning("Create role failed: role name already exists [{RoleName}] in company [{CompanyId}]", dto.Name, dto.CompanyId);
            return Response<RoleDto>.Fail("A role with this name already exists in this company", ErrorCode.Conflict);
        }

        var role = new Role { Name = dto.Name, Description = dto.Description, CompanyId = dto.CompanyId };

        var created = await roleRepository.AddAsync(role, ct);

        var roleWithPermissions = await roleRepository.GetByIdWithPermissionsAsync(created.Id, ct);

        logger.LogInformation("Role created [{RoleId}] [{RoleName}] [{CompanyId}]", created.Id, created.Name, created.CompanyId);

        return Response<RoleDto>.Ok(MapToDto(roleWithPermissions!));
    }

    public async Task<Response<RoleDto>> UpdateAsync(long id, UpdateRoleDto dto, CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<RoleDto>();
        }

        var role = await roleRepository.GetByIdAsync(id, ct);
        if (role is null)
        {
            logger.LogWarning("Update role failed: role not found [{RoleId}]", id);
            return Response<RoleDto>.Fail("Role not found", ErrorCode.NotFound);
        }

        if (await roleRepository.ExistsByNameAsync(dto.Name, role.CompanyId, id, ct))
        {
            logger.LogWarning("Update role failed: role name already exists [{RoleName}] in company [{CompanyId}]", dto.Name, role.CompanyId);
            return Response<RoleDto>.Fail("A role with this name already exists in this company", ErrorCode.Conflict);
        }

        role.Name = dto.Name;
        role.Description = dto.Description;

        await roleRepository.UpdateAsync(role, ct);

        var roleWithPermissions = await roleRepository.GetByIdWithPermissionsAsync(id, ct);

        logger.LogInformation("Role updated [{RoleId}] [{RoleName}]", id, role.Name);

        return Response<RoleDto>.Ok(MapToDto(roleWithPermissions!));
    }

    public async Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var role = await roleRepository.GetByIdAsync(id, ct);
        if (role is null)
        {
            logger.LogWarning("Delete role failed: role not found [{RoleId}]", id);
            return Response<bool>.Fail("Role not found", ErrorCode.NotFound);
        }

        await roleRepository.DeleteAsync(id, ct);

        logger.LogInformation("Role deleted (soft-delete) [{RoleId}] [{RoleName}]", id, role.Name);

        return Response<bool>.Ok(true);
    }

    private static RoleDto MapToDto(Role role)
    {
        return new RoleDto(
            role.Id,
            role.CompanyId,
            role.Name,
            role.Description,
            role.RolePermissions.Select(rp => new PermissionDto(
                rp.Permission.Id,
                rp.Permission.Name,
                rp.Permission.Description)).ToList(),
            role.CreatedAt,
            role.UpdatedAt
        );
    }
}
