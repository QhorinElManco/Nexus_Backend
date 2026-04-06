using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Application.UseCases.Sales;

public class OrderService(
    IOrderRepository orderRepository,
    IOrderDetailRepository orderDetailRepository,
    ICustomerRepository customerRepository,
    IUserRepository userRepository,
    ISmartInventoryRepository smartInventoryRepository,
    IKardexEntryService kardexEntryService,
    IValidator<CreateOrderDto> createValidator,
    IValidator<UpdateOrderDto> updateValidator,
    IValidator<OrderSearchRequest> searchValidator,
    ILogger<OrderService> logger) : IOrderService
{
    private static readonly HashSet<string> ValidOrderTypes = ["Sale", "Return", "Exchange"];

    private static readonly HashSet<string> ValidStatuses =
        ["Pending", "Confirmed", "InProgress", "Completed", "Cancelled"];

    public async Task<Response<OrderDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdWithDetailsAsync(id, ct);

        if (order is null || order.CompanyId != companyId)
        {
            logger.LogWarning("Order not found [{OrderId}] [{CompanyId}]", id, companyId);
            return Response<OrderDto>.Fail("Order not found", ErrorCode.NotFound);
        }

        return Response<OrderDto>.Ok(MapToDto(order));
    }

    public async Task<Response<IReadOnlyList<OrderDto>>> GetByCompanyAsync(long companyId,
        CancellationToken ct = default)
    {
        var orders = await orderRepository.GetByCompanyAsync(companyId, ct);

        var dtos = new List<OrderDto>();
        foreach (var order in orders)
        {
            var orderWithDetails = await orderRepository.GetByIdWithDetailsAsync(order.Id, ct);
            if (orderWithDetails != null)
            {
                dtos.Add(MapToDto(orderWithDetails));
            }
        }

        return Response<IReadOnlyList<OrderDto>>.Ok(dtos);
    }

    public async Task<ResponsePagination<OrderDto>> SearchAsync(OrderSearchRequest request, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await searchValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponsePagination<OrderDto>();
        }

        var (items, totalCount) = await orderRepository.SearchAsync(request, companyId, ct);

        var dtos = new List<OrderDto>();
        foreach (var order in items)
        {
            var orderWithDetails = await orderRepository.GetByIdWithDetailsAsync(order.Id, ct);
            if (orderWithDetails != null)
            {
                dtos.Add(MapToDto(orderWithDetails));
            }
        }

        return ResponsePagination<OrderDto>.Ok(dtos, request.Page, request.PageSize, totalCount);
    }

    public async Task<Response<OrderDto>> CreateAsync(CreateOrderDto dto, long companyId, long userId,
        CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<OrderDto>();
        }

        // Validate Customer exists and belongs to company
        var customer = await customerRepository.GetByIdAsync(dto.CustomerId, ct);
        if (customer is null || customer.CompanyId != companyId)
        {
            logger.LogWarning("Customer not found [{CustomerId}] [{CompanyId}]", dto.CustomerId, companyId);
            return Response<OrderDto>.Fail("Customer not found", ErrorCode.NotFound);
        }

        // Validate User exists
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null)
        {
            logger.LogWarning("User not found [{UserId}]", userId);
            return Response<OrderDto>.Fail("User not found", ErrorCode.NotFound);
        }

        // Validate OrderType
        if (!ValidOrderTypes.Contains(dto.OrderType))
        {
            return Response<OrderDto>.Fail($"Invalid OrderType. Must be one of: {string.Join(", ", ValidOrderTypes)}",
                ErrorCode.ValidationError);
        }

        var orderType = dto.OrderType ?? "Sale";

        // Pre-validation phase: check stock for Sale orders
        if (orderType == "Sale" && dto.OrderDetails != null && dto.OrderDetails.Count > 0)
        {
            if (!dto.WarehouseId.HasValue)
            {
                return Response<OrderDto>.Fail(
                    "WarehouseId is required for stock validation on Sale orders",
                    ErrorCode.ValidationError);
            }

            foreach (var detail in dto.OrderDetails)
            {
                var stock = await smartInventoryRepository.GetStockAsync(dto.WarehouseId.Value, detail.SkuId, ct);
                if (stock is null)
                {
                    return Response<OrderDto>.Fail(
                        $"No stock record found for SKU [{detail.SkuId}] in warehouse [{dto.WarehouseId.Value}]",
                        ErrorCode.ValidationError);
                }

                if (stock.CurrentQuantity < detail.Quantity)
                {
                    return Response<OrderDto>.Fail(
                        $"Insufficient stock for SKU [{detail.SkuId}]. Requested: {detail.Quantity}, Available: {stock.CurrentQuantity}",
                        ErrorCode.ValidationError);
                }
            }
        }

        var totalAmount = 0m;
        var orderDetails = new List<OrderDetail>();

        if (dto.OrderDetails != null)
        {
            foreach (var detail in dto.OrderDetails)
            {
                var subtotal = detail.Quantity * detail.UnitPrice;
                totalAmount += subtotal;
                orderDetails.Add(new OrderDetail
                {
                    OrderId = 0, // Will be set after order is created
                    SkuId = detail.SkuId,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    Subtotal = subtotal
                });
            }
        }

        var order = new Order
        {
            CompanyId = companyId,
            CustomerId = dto.CustomerId,
            UserId = userId,
            VisitId = dto.VisitId,
            WarehouseId = dto.WarehouseId,
            OrderType = orderType,
            Status = dto.Status ?? "Pending",
            TotalAmount = totalAmount
        };

        var created = await orderRepository.AddAsync(order, ct);

        // Add OrderDetails if provided
        if (orderDetails.Count > 0)
        {
            foreach (var detail in orderDetails)
            {
                detail.OrderId = created.Id;
            }

            foreach (var detail in orderDetails)
            {
                await orderDetailRepository.AddAsync(detail, ct);
            }
        }

        // Post-creation phase: create KardexEntry records for each detail
        if (dto.OrderDetails != null && dto.OrderDetails.Count > 0 && dto.WarehouseId.HasValue)
        {
            // Determine transaction type based on order type
            var transactionType = orderType == "Return" ? "Return" : "Sale";

            foreach (var detail in dto.OrderDetails)
            {
                try
                {
                    await kardexEntryService.CreateEntryAsync(
                        companyId,
                        dto.WarehouseId.Value,
                        detail.SkuId,
                        userId,
                        transactionType,
                        detail.Quantity,
                        "Order",
                        created.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        ct: ct);
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogError(ex, "Failed to create KardexEntry for order [{OrderId}] detail [SKU:{SkuId}]",
                        created.Id, detail.SkuId);
                    // KardexEntry creation failure is logged but doesn't block order creation
                    // This could be handled with a saga pattern in production
                }
            }
        }

        var orderWithDetails = await orderRepository.GetByIdWithDetailsAsync(created.Id, ct);

        logger.LogInformation("Order created [{OrderId}] [{CustomerId}] [{TotalAmount}]",
            created.Id, created.CustomerId, created.TotalAmount);

        return Response<OrderDto>.Ok(MapToDto(orderWithDetails!));
    }

    public async Task<Response<OrderDto>> UpdateAsync(long id, UpdateOrderDto dto, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<OrderDto>();
        }

        var order = await orderRepository.GetByIdAsync(id, ct);
        if (order is null || order.CompanyId != companyId)
        {
            logger.LogWarning("Order not found [{OrderId}] [{CompanyId}]", id, companyId);
            return Response<OrderDto>.Fail("Order not found", ErrorCode.NotFound);
        }

        // Validate and apply updates
        if (dto.OrderType != null)
        {
            if (!ValidOrderTypes.Contains(dto.OrderType))
            {
                return Response<OrderDto>.Fail(
                    $"Invalid OrderType. Must be one of: {string.Join(", ", ValidOrderTypes)}",
                    ErrorCode.ValidationError);
            }

            order.OrderType = dto.OrderType;
        }

        if (dto.Status != null)
        {
            if (!ValidStatuses.Contains(dto.Status))
            {
                return Response<OrderDto>.Fail($"Invalid Status. Must be one of: {string.Join(", ", ValidStatuses)}",
                    ErrorCode.ValidationError);
            }

            order.Status = dto.Status;
        }

        if (dto.VisitId.HasValue)
        {
            order.VisitId = dto.VisitId;
        }

        if (dto.WarehouseId.HasValue)
        {
            order.WarehouseId = dto.WarehouseId;
        }

        await orderRepository.UpdateAsync(order, ct);

        var orderWithDetails = await orderRepository.GetByIdWithDetailsAsync(id, ct);

        logger.LogInformation("Order updated [{OrderId}] [{Status}]", id, order.Status);

        return Response<OrderDto>.Ok(MapToDto(orderWithDetails!));
    }

    public async Task<Response<bool>> DeleteAsync(long id, long companyId, CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(id, ct);
        if (order is null || order.CompanyId != companyId)
        {
            logger.LogWarning("Order not found [{OrderId}] [{CompanyId}]", id, companyId);
            return Response<bool>.Fail("Order not found", ErrorCode.NotFound);
        }

        // Soft delete the order
        await orderRepository.DeleteAsync(id, ct);

        // Soft delete related OrderDetails
        var details = await orderDetailRepository.GetByOrderIdAsync(id, ct);
        foreach (var detail in details)
        {
            await orderDetailRepository.DeleteAsync(detail.Id, ct);
        }

        logger.LogInformation("Order deleted (soft-delete) [{OrderId}]", id);

        return Response<bool>.Ok(true);
    }

    public async Task<Response<OrderDto>> AddDetailAsync(long orderId, CreateOrderDetailDto dto, long companyId,
        CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, ct);
        if (order is null || order.CompanyId != companyId)
        {
            logger.LogWarning("Order not found [{OrderId}] [{CompanyId}]", orderId, companyId);
            return Response<OrderDto>.Fail("Order not found", ErrorCode.NotFound);
        }

        if (order.Status == "Completed" || order.Status == "Cancelled")
        {
            return Response<OrderDto>.Fail("Cannot add details to a completed or cancelled order",
                ErrorCode.ValidationError);
        }

        var subtotal = dto.Quantity * dto.UnitPrice;

        var orderDetail = new OrderDetail
        {
            OrderId = orderId,
            SkuId = dto.SkuId,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            Subtotal = subtotal
        };

        await orderDetailRepository.AddAsync(orderDetail, ct);

        // Recalculate TotalAmount
        order.TotalAmount += subtotal;
        await orderRepository.UpdateAsync(order, ct);

        var orderWithDetails = await orderRepository.GetByIdWithDetailsAsync(orderId, ct);

        logger.LogInformation("Order detail added to order [{OrderId}] [{DetailId}] [{Subtotal}]",
            orderId, orderDetail.Id, subtotal);

        return Response<OrderDto>.Ok(MapToDto(orderWithDetails!));
    }

    public async Task<Response<bool>> RemoveDetailAsync(long orderId, long detailId, long companyId,
        CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, ct);
        if (order is null || order.CompanyId != companyId)
        {
            logger.LogWarning("Order not found [{OrderId}] [{CompanyId}]", orderId, companyId);
            return Response<bool>.Fail("Order not found", ErrorCode.NotFound);
        }

        if (order.Status == "Completed" || order.Status == "Cancelled")
        {
            return Response<bool>.Fail("Cannot remove details from a completed or cancelled order",
                ErrorCode.ValidationError);
        }

        var detail = await orderDetailRepository.GetByIdAsync(detailId, ct);
        if (detail is null || detail.OrderId != orderId)
        {
            logger.LogWarning("Order detail not found [{DetailId}] [{OrderId}]", detailId, orderId);
            return Response<bool>.Fail("Order detail not found", ErrorCode.NotFound);
        }

        // Subtract from TotalAmount and soft delete
        order.TotalAmount -= detail.Subtotal;
        await orderRepository.UpdateAsync(order, ct);

        await orderDetailRepository.DeleteAsync(detailId, ct);

        logger.LogInformation("Order detail removed from order [{OrderId}] [{DetailId}]", orderId, detailId);

        return Response<bool>.Ok(true);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.CompanyId,
            order.CustomerId,
            order.Customer.Name,
            order.UserId,
            order.User.FullName,
            order.VisitId,
            order.WarehouseId,
            order.OrderType,
            order.Status,
            order.TotalAmount,
            order.OrderDetails.Select(od => new OrderDetailDto(
                od.Id,
                od.OrderId,
                od.SkuId,
                od.Sku.Barcode,
                od.Sku.Product.Name,
                od.Quantity,
                od.UnitPrice,
                od.Subtotal
            )).ToList(),
            order.CreatedAt,
            order.UpdatedAt
        );
    }
}
