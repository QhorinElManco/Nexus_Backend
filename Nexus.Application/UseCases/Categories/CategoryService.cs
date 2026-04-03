using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Products;

namespace Nexus.Application.UseCases.Categories;

public class CategoryService(
    ICategoryRepository categoryRepository,
    ICompanyRepository companyRepository,
    IValidator<CreateCategoryDto> createValidator,
    IValidator<UpdateCategoryDto> updateValidator,
    ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<Response<CategoryDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, ct);

        if (category is null || category.CompanyId != companyId)
        {
            return Response<CategoryDto>.Fail("Category not found", ErrorCode.NotFound);
        }

        return Response<CategoryDto>.Ok(MapToDto(category));
    }

    public async Task<Response<IReadOnlyList<CategoryDto>>> GetByCompanyAsync(long companyId,
        CancellationToken ct = default)
    {
        var categories = await categoryRepository.GetByCompanyAsync(companyId, ct);
        return Response<IReadOnlyList<CategoryDto>>.Ok(categories.Select(MapToDto).ToList());
    }

    public async Task<Response<CategoryDto>> CreateAsync(CreateCategoryDto dto, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<CategoryDto>();
        }

        var companyExists = await companyRepository.GetByIdAsync(companyId, ct);
        if (companyExists is null)
        {
            logger.LogWarning("Create category failed: company not found [{CompanyId}]", companyId);
            return Response<CategoryDto>.Fail("Company not found", ErrorCode.NotFound);
        }

        if (await categoryRepository.ExistsByNameAsync(companyId, dto.Name, ct: ct))
        {
            logger.LogWarning("Create category failed: category name already exists [{Name}] for company [{CompanyId}]",
                dto.Name, companyId);
            return Response<CategoryDto>.Fail("A category with this name already exists for this company",
                ErrorCode.Conflict);
        }

        var category = new Category { CompanyId = companyId, Name = dto.Name, Description = dto.Description };

        var created = await categoryRepository.AddAsync(category, ct);

        logger.LogInformation("Category created [{CategoryId}] [{Name}] [{CompanyId}]", created.Id, created.Name,
            created.CompanyId);

        return Response<CategoryDto>.Ok(MapToDto(created));
    }

    public async Task<Response<CategoryDto>> UpdateAsync(long id, UpdateCategoryDto dto, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<CategoryDto>();
        }

        var category = await categoryRepository.GetByIdAsync(id, ct);
        if (category is null || category.CompanyId != companyId)
        {
            logger.LogWarning("Update category failed: category not found [{CategoryId}]", id);
            return Response<CategoryDto>.Fail("Category not found", ErrorCode.NotFound);
        }

        if (await categoryRepository.ExistsByNameAsync(category.CompanyId, dto.Name, id, ct))
        {
            logger.LogWarning("Update category failed: category name already exists [{Name}] for company [{CompanyId}]",
                dto.Name, category.CompanyId);
            return Response<CategoryDto>.Fail("A category with this name already exists for this company",
                ErrorCode.Conflict);
        }

        category.Name = dto.Name;
        category.Description = dto.Description;

        await categoryRepository.UpdateAsync(category, ct);

        logger.LogInformation("Category updated [{CategoryId}] [{Name}]", id, category.Name);

        return Response<CategoryDto>.Ok(MapToDto(category));
    }

    public async Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, ct);
        if (category is null || category.CompanyId != companyId)
        {
            logger.LogWarning("Delete category failed: category not found [{CategoryId}]", id);
            return Response<bool>.Fail("Category not found", ErrorCode.NotFound);
        }

        await categoryRepository.DeleteAsync(id, ct);

        logger.LogInformation("Category deleted (soft-delete) [{CategoryId}] [{Name}]", id, category.Name);

        return Response<bool>.Ok(true);
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto(
            category.Id,
            category.CompanyId,
            category.Name,
            category.Description,
            category.CreatedAt,
            category.UpdatedAt
        );
    }
}
