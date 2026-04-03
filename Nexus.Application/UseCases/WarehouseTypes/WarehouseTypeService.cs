using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Products;

namespace Nexus.Application.UseCases.WarehouseTypes;

public class WarehouseTypeService(
    IWarehouseTypeRepository warehouseTypeRepository,
    ICompanyRepository companyRepository,
    IValidator<CreateWarehouseTypeDto> createValidator,
    IValidator<UpdateWarehouseTypeDto> updateValidator,
    ILogger<WarehouseTypeService> logger) : IWarehouseTypeService
{
    public async Task<Response<WarehouseTypeDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var warehouseType = await warehouseTypeRepository.GetByIdAsync(id, ct);

        if (warehouseType is null || warehouseType.CompanyId != companyId)
        {
            return Response<WarehouseTypeDto>.Fail("WarehouseType not found", ErrorCode.NotFound);
        }

        return Response<WarehouseTypeDto>.Ok(MapToDto(warehouseType));
    }

    public async Task<Response<IReadOnlyList<WarehouseTypeDto>>> GetByCompanyAsync(long companyId,
        CancellationToken ct = default)
    {
        var warehouseTypes = await warehouseTypeRepository.GetByCompanyAsync(companyId, ct);
        return Response<IReadOnlyList<WarehouseTypeDto>>.Ok(warehouseTypes.Select(MapToDto).ToList());
    }

    public async Task<Response<WarehouseTypeDto>> CreateAsync(CreateWarehouseTypeDto dto, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<WarehouseTypeDto>();
        }

        var companyExists = await companyRepository.GetByIdAsync(companyId, ct);
        if (companyExists is null)
        {
            logger.LogWarning("Create warehouse type failed: company not found [{CompanyId}]", companyId);
            return Response<WarehouseTypeDto>.Fail("Company not found", ErrorCode.NotFound);
        }

        if (await warehouseTypeRepository.ExistsByNameAsync(companyId, dto.Name, ct: ct))
        {
            logger.LogWarning(
                "Create warehouse type failed: warehouse type name already exists [{Name}] for company [{CompanyId}]",
                dto.Name, companyId);
            return Response<WarehouseTypeDto>.Fail("A warehouse type with this name already exists for this company",
                ErrorCode.Conflict);
        }

        var warehouseType = new WarehouseType { CompanyId = companyId, Name = dto.Name, Description = dto.Description };

        var created = await warehouseTypeRepository.AddAsync(warehouseType, ct);

        logger.LogInformation("WarehouseType created [{WarehouseTypeId}] [{Name}] [{CompanyId}]", created.Id,
            created.Name, created.CompanyId);

        return Response<WarehouseTypeDto>.Ok(MapToDto(created));
    }

    public async Task<Response<WarehouseTypeDto>> UpdateAsync(long id, UpdateWarehouseTypeDto dto, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<WarehouseTypeDto>();
        }

        var warehouseType = await warehouseTypeRepository.GetByIdAsync(id, ct);
        if (warehouseType is null || warehouseType.CompanyId != companyId)
        {
            logger.LogWarning("Update warehouse type failed: warehouse type not found [{WarehouseTypeId}]", id);
            return Response<WarehouseTypeDto>.Fail("WarehouseType not found", ErrorCode.NotFound);
        }

        if (await warehouseTypeRepository.ExistsByNameAsync(warehouseType.CompanyId, dto.Name, id, ct))
        {
            logger.LogWarning(
                "Update warehouse type failed: warehouse type name already exists [{Name}] for company [{CompanyId}]",
                dto.Name, warehouseType.CompanyId);
            return Response<WarehouseTypeDto>.Fail("A warehouse type with this name already exists for this company",
                ErrorCode.Conflict);
        }

        warehouseType.Name = dto.Name;
        warehouseType.Description = dto.Description;

        await warehouseTypeRepository.UpdateAsync(warehouseType, ct);

        logger.LogInformation("WarehouseType updated [{WarehouseTypeId}] [{Name}]", id, warehouseType.Name);

        return Response<WarehouseTypeDto>.Ok(MapToDto(warehouseType));
    }

    public async Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default)
    {
        var warehouseType = await warehouseTypeRepository.GetByIdAsync(id, ct);
        if (warehouseType is null || warehouseType.CompanyId != companyId)
        {
            logger.LogWarning("Delete warehouse type failed: warehouse type not found [{WarehouseTypeId}]", id);
            return Response<bool>.Fail("WarehouseType not found", ErrorCode.NotFound);
        }

        if (await warehouseTypeRepository.HasWarehousesAsync(id, ct))
        {
            logger.LogWarning("Delete warehouse type failed: warehouse type has warehouses [{WarehouseTypeId}]", id);
            return Response<bool>.Fail("Cannot delete warehouse type: there are warehouses associated with it",
                ErrorCode.Conflict);
        }

        await warehouseTypeRepository.DeleteAsync(id, ct);

        logger.LogInformation("WarehouseType deleted (soft-delete) [{WarehouseTypeId}] [{Name}]", id,
            warehouseType.Name);

        return Response<bool>.Ok(true);
    }

    private static WarehouseTypeDto MapToDto(WarehouseType warehouseType)
    {
        return new WarehouseTypeDto(
            warehouseType.Id,
            warehouseType.CompanyId,
            warehouseType.Name,
            warehouseType.Description,
            warehouseType.CreatedAt,
            warehouseType.UpdatedAt
        );
    }
}
