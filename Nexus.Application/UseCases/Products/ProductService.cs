using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Products;

namespace Nexus.Application.UseCases.Products;

public class ProductService(
    IProductRepository productRepository,
    ICompanyRepository companyRepository,
    ICategoryRepository categoryRepository,
    IValidator<CreateProductDto> createValidator,
    IValidator<UpdateProductDto> updateValidator,
    ILogger<ProductService> logger) : IProductService
{
    public async Task<Response<ProductDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdWithCategoryAsync(id, ct);

        return product is null
            ? Response<ProductDto>.Fail("Product not found", ErrorCode.NotFound)
            : Response<ProductDto>.Ok(MapToDto(product));
    }

    public async Task<Response<IReadOnlyList<ProductDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var products = await productRepository.GetAllWithCategoryAsync(ct);
        return Response<IReadOnlyList<ProductDto>>.Ok(products.Select(MapToDto).ToList());
    }

    public async Task<Response<IReadOnlyList<ProductDto>>> GetByCompanyAsync(long companyId, CancellationToken ct = default)
    {
        var products = await productRepository.GetByCompanyWithCategoryAsync(companyId, ct);
        return Response<IReadOnlyList<ProductDto>>.Ok(products.Select(MapToDto).ToList());
    }

    public async Task<Response<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<ProductDto>();
        }

        var companyExists = await companyRepository.GetByIdAsync(dto.CompanyId, ct);
        if (companyExists is null)
        {
            logger.LogWarning("Create product failed: company not found [{CompanyId}]", dto.CompanyId);
            return Response<ProductDto>.Fail("Company not found", ErrorCode.NotFound);
        }

        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await categoryRepository.GetByIdAsync(dto.CategoryId.Value, ct);
            if (categoryExists is null)
            {
                logger.LogWarning("Create product failed: category not found [{CategoryId}]", dto.CategoryId.Value);
                return Response<ProductDto>.Fail("Category not found", ErrorCode.NotFound);
            }
        }

        if (await productRepository.ExistsByNameAsync(dto.CompanyId, dto.Name, ct: ct))
        {
            logger.LogWarning("Create product failed: product name already exists [{Name}] for company [{CompanyId}]",
                dto.Name, dto.CompanyId);
            return Response<ProductDto>.Fail("A product with this name already exists for this company", ErrorCode.Conflict);
        }

        var product = new Product
        {
            CompanyId = dto.CompanyId,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Brand = dto.Brand
        };

        var created = await productRepository.AddAsync(product, ct);

        logger.LogInformation("Product created [{ProductId}] [{Name}] [{CompanyId}] [{CategoryId}]", 
            created.Id, created.Name, created.CompanyId, created.CategoryId);

        return Response<ProductDto>.Ok(MapToDto(created));
    }

    public async Task<Response<ProductDto>> UpdateAsync(long id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<ProductDto>();
        }

        var product = await productRepository.GetByIdWithCategoryAsync(id, ct);
        if (product is null)
        {
            logger.LogWarning("Update product failed: product not found [{ProductId}]", id);
            return Response<ProductDto>.Fail("Product not found", ErrorCode.NotFound);
        }

        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await categoryRepository.GetByIdAsync(dto.CategoryId.Value, ct);
            if (categoryExists is null)
            {
                logger.LogWarning("Update product failed: category not found [{CategoryId}]", dto.CategoryId.Value);
                return Response<ProductDto>.Fail("Category not found", ErrorCode.NotFound);
            }
        }

        if (await productRepository.ExistsByNameAsync(product.CompanyId, dto.Name, id, ct))
        {
            logger.LogWarning("Update product failed: product name already exists [{Name}] for company [{CompanyId}]",
                dto.Name, product.CompanyId);
            return Response<ProductDto>.Fail("A product with this name already exists for this company", ErrorCode.Conflict);
        }

        product.CategoryId = dto.CategoryId;
        product.Name = dto.Name;
        product.Brand = dto.Brand;

        await productRepository.UpdateAsync(product, ct);

        logger.LogInformation("Product updated [{ProductId}] [{Name}]", id, product.Name);

        return Response<ProductDto>.Ok(MapToDto(product));
    }

    public async Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var product = await productRepository.GetByIdAsync(id, ct);
        if (product is null)
        {
            logger.LogWarning("Delete product failed: product not found [{ProductId}]", id);
            return Response<bool>.Fail("Product not found", ErrorCode.NotFound);
        }

        await productRepository.DeleteAsync(id, ct);

        logger.LogInformation("Product deleted (soft-delete) [{ProductId}] [{Name}]", id, product.Name);

        return Response<bool>.Ok(true);
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.CompanyId,
            product.CategoryId,
            product.Category?.Name,
            product.Name,
            product.Brand,
            product.CreatedAt,
            product.UpdatedAt
        );
    }
}