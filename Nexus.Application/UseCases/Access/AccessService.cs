using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Access;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;

namespace Nexus.Application.UseCases.Access;

public class AccessService(
    IAccessRepository accessRepository,
    IValidator<CreateAccessDto> createValidator,
    IValidator<UpdateAccessDto> updateValidator,
    ILogger<AccessService> logger) : IAccessService
{
    public async Task<Response<AccessDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var access = await accessRepository.GetByIdWithRolesAsync(id, ct);

        return access is null
            ? Response<AccessDto>.Fail("Access not found", ErrorCode.NotFound)
            : Response<AccessDto>.Ok(MapToDto(access));
    }

    public async Task<Response<IReadOnlyList<AccessDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var accesses = await accessRepository.GetAllAsync(ct);
        return Response<IReadOnlyList<AccessDto>>.Ok(accesses.Select(MapToDto).ToList());
    }

    public async Task<Response<AccessDto>> CreateAsync(CreateAccessDto dto, CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<AccessDto>();
        }

        if (await accessRepository.ExistsByNameAsync(dto.Name, ct: ct))
        {
            logger.LogWarning("Create access failed: access name already exists [{AccessName}]", dto.Name);
            return Response<AccessDto>.Fail("An access with this name already exists", ErrorCode.Conflict);
        }

        var access = new Domain.Entities.Security.Access { Name = dto.Name, Description = dto.Description };

        var created = await accessRepository.AddAsync(access, ct);

        var accessWithRoles = await accessRepository.GetByIdWithRolesAsync(created.Id, ct);

        logger.LogInformation("Access created [{AccessId}] [{AccessName}]", created.Id, created.Name);

        return Response<AccessDto>.Ok(MapToDto(accessWithRoles!));
    }

    public async Task<Response<AccessDto>> UpdateAsync(long id, UpdateAccessDto dto, CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<AccessDto>();
        }

        var access = await accessRepository.GetByIdAsync(id, ct);
        if (access is null)
        {
            logger.LogWarning("Update access failed: access not found [{AccessId}]", id);
            return Response<AccessDto>.Fail("Access not found", ErrorCode.NotFound);
        }

        if (await accessRepository.ExistsByNameAsync(dto.Name, id, ct))
        {
            logger.LogWarning("Update access failed: access name already exists [{AccessName}]", dto.Name);
            return Response<AccessDto>.Fail("An access with this name already exists", ErrorCode.Conflict);
        }

        access.Name = dto.Name;
        access.Description = dto.Description;

        await accessRepository.UpdateAsync(access, ct);

        var accessWithRoles = await accessRepository.GetByIdWithRolesAsync(id, ct);

        logger.LogInformation("Access updated [{AccessId}] [{AccessName}]", id, access.Name);

        return Response<AccessDto>.Ok(MapToDto(accessWithRoles!));
    }

    public async Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var access = await accessRepository.GetByIdAsync(id, ct);
        if (access is null)
        {
            logger.LogWarning("Delete access failed: access not found [{AccessId}]", id);
            return Response<bool>.Fail("Access not found", ErrorCode.NotFound);
        }

        await accessRepository.DeleteAsync(id, ct);

        logger.LogInformation("Access deleted (soft-delete) [{AccessId}] [{AccessName}]", id, access.Name);

        return Response<bool>.Ok(true);
    }

    private static AccessDto MapToDto(Domain.Entities.Security.Access access)
    {
        return new AccessDto(
            access.Id,
            access.Name,
            access.Description,
            access.RolePermissions.Select(rp => new RoleSummaryDto(
                rp.Role.Id,
                rp.Role.Name)).ToList(),
            access.CreatedAt,
            access.UpdatedAt
        );
    }
}
