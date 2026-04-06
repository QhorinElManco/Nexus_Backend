using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Application.UseCases.Sales;

public class DeliveryService(
    IDeliveryRepository deliveryRepository,
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    IKardexEntryService kardexEntryService,
    IValidator<CreateDeliveryDto> createValidator,
    IValidator<UpdateDeliveryDto> updateValidator,
    IValidator<DeliverySearchRequest> searchValidator,
    ILogger<DeliveryService> logger) : IDeliveryService
{
    private static readonly HashSet<string> _validStatuses = ["Pending", "InTransit", "Delivered", "Failed"];

    public async Task<Response<DeliveryDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var delivery = await deliveryRepository.GetByIdAsync(id, ct);

        if (delivery is null || delivery.CompanyId != companyId)
        {
            logger.LogWarning("Delivery not found [{DeliveryId}] [{CompanyId}]", id, companyId);
            return Response<DeliveryDto>.Fail("Delivery not found", ErrorCode.NotFound);
        }

        return Response<DeliveryDto>.Ok(MapToDto(delivery));
    }

    public async Task<Response<IReadOnlyList<DeliveryDto>>> GetByCompanyAsync(long companyId,
        CancellationToken ct = default)
    {
        var deliveries = await deliveryRepository.GetByCompanyAsync(companyId, ct);
        return Response<IReadOnlyList<DeliveryDto>>.Ok(deliveries.Select(MapToDto).ToList());
    }

    public async Task<ResponsePagination<DeliveryDto>> SearchAsync(DeliverySearchRequest request, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await searchValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponsePagination<DeliveryDto>();
        }

        var (items, totalCount) = await deliveryRepository.SearchAsync(request, companyId, ct);

        return ResponsePagination<DeliveryDto>.Ok(
            items.Select(MapToDto).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    public async Task<Response<DeliveryDto>> CreateAsync(CreateDeliveryDto dto, long companyId, long userId,
        CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<DeliveryDto>();
        }

        // Validate Order exists and belongs to company
        var order = await orderRepository.GetByIdAsync(dto.OrderId, ct);
        if (order is null || order.CompanyId != companyId)
        {
            logger.LogWarning("Order not found [{OrderId}] [{CompanyId}]", dto.OrderId, companyId);
            return Response<DeliveryDto>.Fail("Order not found", ErrorCode.NotFound);
        }

        // Validate User exists
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null)
        {
            logger.LogWarning("User not found [{UserId}]", userId);
            return Response<DeliveryDto>.Fail("User not found", ErrorCode.NotFound);
        }

        var delivery = new Delivery
        {
            CompanyId = companyId, OrderId = dto.OrderId, UserId = userId, Status = dto.Status ?? "Pending"
        };

        var created = await deliveryRepository.AddAsync(delivery, ct);

        logger.LogInformation("Delivery created [{DeliveryId}] [{OrderId}]", created.Id, created.OrderId);

        var deliveryWithRelations = await deliveryRepository.GetByIdAsync(created.Id, ct);
        return Response<DeliveryDto>.Ok(MapToDto(deliveryWithRelations!));
    }

    public async Task<Response<DeliveryDto>> UpdateAsync(long id, UpdateDeliveryDto dto, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<DeliveryDto>();
        }

        var delivery = await deliveryRepository.GetByIdAsync(id, ct);
        if (delivery is null || delivery.CompanyId != companyId)
        {
            logger.LogWarning("Delivery not found [{DeliveryId}] [{CompanyId}]", id, companyId);
            return Response<DeliveryDto>.Fail("Delivery not found", ErrorCode.NotFound);
        }

        // Check if status is transitioning to "Delivered"
        var isTransitioningToDelivered = dto.Status == "Delivered" && delivery.Status != "Delivered";

        // Validate and apply updates
        if (dto.Status != null)
        {
            if (!_validStatuses.Contains(dto.Status))
            {
                return Response<DeliveryDto>.Fail(
                    $"Invalid Status. Must be one of: {string.Join(", ", _validStatuses)}",
                    ErrorCode.ValidationError);
            }

            // Check for duplicate delivery
            if (dto.Status == "Delivered" && delivery.Status == "Delivered")
            {
                return Response<DeliveryDto>.Fail("Delivery is already marked as delivered",
                    ErrorCode.ValidationError);
            }

            delivery.Status = dto.Status;
        }

        if (dto.DeliveryTime.HasValue)
        {
            delivery.DeliveryTime = dto.DeliveryTime;
        }

        if (dto.DeliveryLat.HasValue)
        {
            delivery.DeliveryLat = dto.DeliveryLat;
        }

        if (dto.DeliveryLng.HasValue)
        {
            delivery.DeliveryLng = dto.DeliveryLng;
        }

        if (dto.ProofOfDeliveryUrl != null)
        {
            delivery.ProofOfDeliveryUrl = dto.ProofOfDeliveryUrl;
        }

        await deliveryRepository.UpdateAsync(delivery, ct);

        // If transitioning to Delivered, add stock and create KardexEntry records
        if (isTransitioningToDelivered)
        {
            var order = await orderRepository.GetByIdWithDetailsAsync(delivery.OrderId, ct);
            if (order is { OrderDetails.Count: > 0, WarehouseId: not null })
            {
                foreach (var detail in order.OrderDetails)
                {
                    try
                    {
                        await kardexEntryService.CreateEntryAsync(
                            companyId,
                            order.WarehouseId.Value,
                            detail.SkuId,
                            delivery.UserId,
                            "Purchase",
                            detail.Quantity,
                            "Delivery",
                            delivery.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            ct: ct);
                    }
                    catch (InvalidOperationException ex)
                    {
                        logger.LogError(ex,
                            "Failed to create KardexEntry for delivery [{DeliveryId}] detail [SKU:{SkuId}]",
                            delivery.Id, detail.SkuId);
                    }
                }
            }
        }

        var deliveryWithRelations = await deliveryRepository.GetByIdAsync(id, ct);

        logger.LogInformation("Delivery updated [{DeliveryId}] [{Status}]", id, delivery.Status);

        return Response<DeliveryDto>.Ok(MapToDto(deliveryWithRelations!));
    }

    private static DeliveryDto MapToDto(Delivery delivery)
    {
        return new DeliveryDto(
            delivery.Id,
            delivery.CompanyId,
            delivery.OrderId,
            delivery.Order.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            delivery.UserId,
            delivery.User.FullName,
            delivery.DeliveryTime,
            delivery.DeliveryLat,
            delivery.DeliveryLng,
            delivery.Status,
            delivery.ProofOfDeliveryUrl,
            delivery.CreatedAt,
            delivery.UpdatedAt
        );
    }
}
