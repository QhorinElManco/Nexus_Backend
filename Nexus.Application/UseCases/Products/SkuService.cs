using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Products;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Products;

namespace Nexus.Application.UseCases.Products;

public class SkuService(
    ISkuRepository skuRepository,
    IProductRepository productRepository,
    IValidator<CreateSkuDto> createValidator,
    IValidator<UpdateSkuDto> updateValidator,
    ILogger<SkuService> logger) : ISkuService
{
    public async Task<Response<SkuDto>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var sku = await skuRepository.GetByIdWithProductAsync(id, ct);

        return sku is null
            ? Response<SkuDto>.Fail("Sku not found", ErrorCode.NotFound)
            : Response<SkuDto>.Ok(MapToDto(sku));
    }

    public async Task<Response<IReadOnlyList<SkuDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var skus = await skuRepository.GetAllAsync(ct);
        return Response<IReadOnlyList<SkuDto>>.Ok(skus.Select(s => MapToDto(s)).ToList());
    }

    public async Task<Response<IReadOnlyList<SkuDto>>> GetByProductAsync(long productId, CancellationToken ct = default)
    {
        var skus = await skuRepository.GetByProductAsync(productId, ct);
        return Response<IReadOnlyList<SkuDto>>.Ok(skus.Select(s => MapToDto(s)).ToList());
    }

    public async Task<Response<SkuDto>> CreateAsync(CreateSkuDto dto, CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<SkuDto>();
        }

        var productExists = await productRepository.GetByIdAsync(dto.ProductId, ct);
        if (productExists is null)
        {
            logger.LogWarning("Create sku failed: product not found [{ProductId}]", dto.ProductId);
            return Response<SkuDto>.Fail("Product not found", ErrorCode.NotFound);
        }

        var sku = new Sku
        {
            ProductId = dto.ProductId,
            Barcode = dto.Barcode,
            UnitMeasure = dto.UnitMeasure,
            BasePrice = dto.BasePrice,
            IsActive = true
        };

        var created = await skuRepository.AddAsync(sku, ct);

        logger.LogInformation("Sku created [{SkuId}] [{Barcode}] [{ProductId}]", 
            created.Id, created.Barcode, created.ProductId);

        return Response<SkuDto>.Ok(MapToDto(created, productExists.Name));
    }

    public async Task<Response<SkuDto>> UpdateAsync(long id, UpdateSkuDto dto, CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<SkuDto>();
        }

        var sku = await skuRepository.GetByIdWithProductAsync(id, ct);
        if (sku is null)
        {
            logger.LogWarning("Update sku failed: sku not found [{SkuId}]", id);
            return Response<SkuDto>.Fail("Sku not found", ErrorCode.NotFound);
        }

        sku.Barcode = dto.Barcode;
        sku.UnitMeasure = dto.UnitMeasure;
        sku.BasePrice = dto.BasePrice;

        await skuRepository.UpdateAsync(sku, ct);

        logger.LogInformation("Sku updated [{SkuId}] [{Barcode}]", id, sku.Barcode);

        return Response<SkuDto>.Ok(MapToDto(sku));
    }

    public async Task<Response<bool>> DeleteAsync(long id, CancellationToken ct = default)
    {
        var sku = await skuRepository.GetByIdAsync(id, ct);
        if (sku is null)
        {
            logger.LogWarning("Delete sku failed: sku not found [{SkuId}]", id);
            return Response<bool>.Fail("Sku not found", ErrorCode.NotFound);
        }

        await skuRepository.DeleteAsync(id, ct);

        logger.LogInformation("Sku deleted (soft-delete) [{SkuId}] [{Barcode}]", id, sku.Barcode);

        return Response<bool>.Ok(true);
    }

    private static SkuDto MapToDto(Sku sku, string? productName = null)
    {
        return new SkuDto(
            sku.Id,
            sku.ProductId,
            productName ?? sku.Product?.Name ?? string.Empty,
            sku.Barcode,
            sku.UnitMeasure,
            sku.BasePrice,
            sku.IsActive,
            sku.CreatedAt,
            sku.UpdatedAt
        );
    }
}