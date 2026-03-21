using FluentValidation;
using Nexus.Application.Dto.Access;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Security;

namespace Nexus.Application.UseCases.Access;

public class AccessService(
    IAccessRepository accessRepository,
    IValidator<CreateAccessDto> createValidator,
    IValidator<UpdateAccessDto> updateValidator) : IAccessService
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
            return Response<AccessDto>.Fail("An access with this name already exists", ErrorCode.Conflict);
        }

        var access = new Domain.Entities.Security.Access { Name = dto.Name, Description = dto.Description };

        var created = await accessRepository.AddAsync(access, ct);

        var accessWithRoles = await accessRepository.GetByIdWithRolesAsync(created.Id, ct);
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
            return Response<AccessDto>.Fail("Access not found", ErrorCode.NotFound);
        }

        if (await accessRepository.ExistsByNameAsync(dto.Name, id, ct))
        {
            return Response<AccessDto>.Fail("An access with this name already exists", ErrorCode.Conflict);
        }

        access.Name = dto.Name;
        access.Description = dto.Description;

        await accessRepository.UpdateAsync(access, ct);

        var accessWithRoles = await accessRepository.GetByIdWithRolesAsync(id, ct);
        return Response<AccessDto>.Ok(MapToDto(accessWithRoles!));
    }

    public async Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var access = await accessRepository.GetByIdAsync(id, ct);
        if (access is null)
        {
            return Response<bool>.Fail("Access not found", ErrorCode.NotFound);
        }

        await accessRepository.DeleteAsync(id, ct);
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
