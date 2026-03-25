using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Products;

namespace Nexus.Application.UseCases.Products.Warehouses;

public class WarehouseService(
    IWarehouseRepository warehouseRepository,
    IWarehouseTypeRepository warehouseTypeRepository,
    ICompanyRepository companyRepository,
    IUserRepository userRepository,
    IValidator<CreateWarehouseDto> createValidator,
    IValidator<UpdateWarehouseDto> updateValidator,
    ILogger<WarehouseService> logger) : IWarehouseService
{
    public async Task<Response<WarehouseDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(id, ct);

        if (warehouse is not null && warehouse.CompanyId == companyId)
        {
            return Response<WarehouseDto>.Ok(MapToDto(warehouse));
        }

        logger.LogWarning("Get warehouse failed: warehouse not found [{WarehouseId}] [{CompanyId}]", id, companyId);
        return Response<WarehouseDto>.Fail("Warehouse not found", ErrorCode.NotFound);
    }

    public async Task<Response<IReadOnlyList<WarehouseDto>>> GetAllAsync(long companyId, CancellationToken ct = default)
    {
        var warehouses = await warehouseRepository.GetByCompanyAsync(companyId, ct);
        return Response<IReadOnlyList<WarehouseDto>>.Ok(warehouses.Select(MapToDto).ToList());
    }

    public async Task<Response<WarehouseDto>> CreateAsync(CreateWarehouseDto dto, CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<WarehouseDto>();
        }

        var company = await companyRepository.GetByIdAsync(dto.CompanyId, ct);
        if (company is null)
        {
            logger.LogWarning("Create warehouse failed: company not found [{CompanyId}]", dto.CompanyId);
            return Response<WarehouseDto>.Fail("Company not found", ErrorCode.NotFound);
        }

        var manager = await userRepository.GetByIdAsync(dto.ManagerId, ct);
        if (manager is null || manager.CompanyId != dto.CompanyId)
        {
            logger.LogWarning(
                "Create warehouse failed: manager not found or not in company [{ManagerId}] [{CompanyId}]",
                dto.ManagerId, dto.CompanyId);
            return Response<WarehouseDto>.Fail("Manager not found or does not belong to the company",
                ErrorCode.NotFound);
        }

        var warehouseType = await warehouseTypeRepository.GetByIdAsync(dto.WarehouseTypeId, ct);
        if (warehouseType is null)
        {
            logger.LogWarning(
                "Create warehouse failed: warehouse type not found [{WarehouseTypeId}]",
                dto.WarehouseTypeId);
            return Response<WarehouseDto>.Fail("WarehouseType not found", ErrorCode.NotFound);
        }

        if (await warehouseRepository.ExistsByNameAsync(dto.CompanyId, dto.Name, null, ct))
        {
            logger.LogWarning("Create warehouse failed: name already exists [{CompanyId}] [{Name}]", dto.CompanyId,
                dto.Name);
            return Response<WarehouseDto>.Fail("A warehouse with this name already exists", ErrorCode.Conflict);
        }

        var warehouse = new Warehouse
        {
            CompanyId = dto.CompanyId,
            ManagerId = dto.ManagerId,
            Name = dto.Name,
            WarehouseTypeId = dto.WarehouseTypeId,
            Lat = dto.Lat,
            Lng = dto.Lng
        };

        var created = await warehouseRepository.AddAsync(warehouse, ct);

        logger.LogInformation("Warehouse created [{WarehouseId}] [{CompanyId}] [{Name}] [{WarehouseTypeId}]",
            created.Id, created.CompanyId, created.Name, created.WarehouseTypeId);

        return Response<WarehouseDto>.Ok(MapToDto(created), "Warehouse created successfully");
    }

    public async Task<Response<WarehouseDto>> UpdateAsync(long id, UpdateWarehouseDto dto,
        CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<WarehouseDto>();
        }

        var warehouse = await warehouseRepository.GetByIdAsync(id, ct);
        if (warehouse is null)
        {
            logger.LogWarning("Update warehouse failed: warehouse not found [{WarehouseId}]", id);
            return Response<WarehouseDto>.Fail("Warehouse not found", ErrorCode.NotFound);
        }

        var companyId = warehouse.CompanyId;

        if (dto.WarehouseTypeId.HasValue)
        {
            var warehouseType = await warehouseTypeRepository.GetByIdAsync(dto.WarehouseTypeId.Value, ct);
            if (warehouseType is null)
            {
                logger.LogWarning(
                    "Update warehouse failed: warehouse type not found [{WarehouseTypeId}]",
                    dto.WarehouseTypeId.Value);
                return Response<WarehouseDto>.Fail("WarehouseType not found", ErrorCode.NotFound);
            }

            warehouse.WarehouseTypeId = dto.WarehouseTypeId.Value;
        }

        if (dto.ManagerId.HasValue)
        {
            var manager = await userRepository.GetByIdAsync(dto.ManagerId.Value, ct);
            if (manager is null || manager.CompanyId != companyId)
            {
                logger.LogWarning(
                    "Update warehouse failed: manager not found or not in company [{ManagerId}] [{CompanyId}]",
                    dto.ManagerId.Value, companyId);
                return Response<WarehouseDto>.Fail("Manager not found or does not belong to the company",
                    ErrorCode.NotFound);
            }

            warehouse.ManagerId = dto.ManagerId.Value;
        }

        if (await warehouseRepository.ExistsByNameAsync(companyId, dto.Name, id, ct))
        {
            logger.LogWarning("Update warehouse failed: name already exists [{CompanyId}] [{Name}]", companyId,
                dto.Name);
            return Response<WarehouseDto>.Fail("A warehouse with this name already exists", ErrorCode.Conflict);
        }

        warehouse.Name = dto.Name;
        warehouse.Lat = dto.Lat;
        warehouse.Lng = dto.Lng;

        await warehouseRepository.UpdateAsync(warehouse, ct);

        logger.LogInformation("Warehouse updated [{WarehouseId}] [{CompanyId}] [{Name}]", id, companyId,
            warehouse.Name);

        return Response<WarehouseDto>.Ok(MapToDto(warehouse), "Warehouse updated successfully");
    }

    public async Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(id, ct);
        if (warehouse is null)
        {
            logger.LogWarning("Delete warehouse failed: warehouse not found [{WarehouseId}]", id);
            return Response<bool>.Fail("Warehouse not found", ErrorCode.NotFound);
        }

        var companyId = warehouse.CompanyId;

        await warehouseRepository.DeleteAsync(id, ct);

        logger.LogInformation("Warehouse deleted (soft-delete) [{WarehouseId}] [{CompanyId}] [{Name}]",
            id, companyId, warehouse.Name);

        return Response<bool>.Ok(true, "Warehouse deleted successfully");
    }

    private static WarehouseDto MapToDto(Warehouse warehouse)
    {
        return new WarehouseDto(
            warehouse.Id,
            warehouse.CompanyId,
            warehouse.Company?.Name,
            warehouse.ManagerId,
            warehouse.Manager?.FullName,
            warehouse.Name,
            warehouse.WarehouseTypeId,
            warehouse.WarehouseType?.Name,
            warehouse.Lat,
            warehouse.Lng,
            warehouse.CreatedAt,
            warehouse.UpdatedAt);
    }
}
