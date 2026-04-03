using FluentValidation;
using Microsoft.Extensions.Logging;
using Nexus.Application.Dto.Sales;
using Nexus.Application.Dto.Response;
using Nexus.Application.Interfaces.Repositories;
using Nexus.Application.Interfaces.UseCases;
using Nexus.Domain.Entities.Sales;

namespace Nexus.Application.UseCases.Sales;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    IUserRepository userRepository,
    IValidator<CreatePaymentDto> createValidator,
    IValidator<PaymentSearchRequest> searchValidator,
    ILogger<PaymentService> logger) : IPaymentService
{
    private static readonly HashSet<string> ValidPaymentMethods = ["Cash", "Card", "Transfer", "Credit"];

    public async Task<Response<PaymentDto>> GetByIdAsync(long id, long companyId, CancellationToken ct = default)
    {
        var payment = await paymentRepository.GetByIdAsync(id, ct);

        if (payment is null || payment.CompanyId != companyId)
        {
            logger.LogWarning("Payment not found [{PaymentId}] [{CompanyId}]", id, companyId);
            return Response<PaymentDto>.Fail("Payment not found", ErrorCode.NotFound);
        }

        return Response<PaymentDto>.Ok(MapToDto(payment));
    }

    public async Task<Response<IReadOnlyList<PaymentDto>>> GetByOrderIdAsync(long orderId, long companyId,
        CancellationToken ct = default)
    {
        var order = await orderRepository.GetByIdAsync(orderId, ct);
        if (order is null || order.CompanyId != companyId)
        {
            logger.LogWarning("Order not found [{OrderId}] [{CompanyId}]", orderId, companyId);
            return Response<IReadOnlyList<PaymentDto>>.Fail("Order not found", ErrorCode.NotFound);
        }

        var payments = await paymentRepository.GetByOrderIdAsync(orderId, ct);
        return Response<IReadOnlyList<PaymentDto>>.Ok(payments.Select(MapToDto).ToList());
    }

    public async Task<ResponsePagination<PaymentDto>> SearchAsync(PaymentSearchRequest request, long companyId,
        CancellationToken ct = default)
    {
        var validationResult = await searchValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponsePagination<PaymentDto>();
        }

        var (items, totalCount) = await paymentRepository.SearchAsync(request, companyId, ct);

        return ResponsePagination<PaymentDto>.Ok(
            items.Select(MapToDto).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }

    public async Task<Response<PaymentDto>> CreateAsync(CreatePaymentDto dto, long companyId, long userId,
        CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, ct);
        if (!validationResult.IsValid)
        {
            return validationResult.ToFailureResponse<PaymentDto>();
        }

        // Validate Order exists and belongs to company
        var order = await orderRepository.GetByIdAsync(dto.OrderId, ct);
        if (order is null || order.CompanyId != companyId)
        {
            logger.LogWarning("Order not found [{OrderId}] [{CompanyId}]", dto.OrderId, companyId);
            return Response<PaymentDto>.Fail("Order not found", ErrorCode.NotFound);
        }

        // Validate User exists
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null)
        {
            logger.LogWarning("User not found [{UserId}]", userId);
            return Response<PaymentDto>.Fail("User not found", ErrorCode.NotFound);
        }

        // Validate PaymentMethod
        if (!ValidPaymentMethods.Contains(dto.PaymentMethod))
        {
            return Response<PaymentDto>.Fail(
                $"Invalid PaymentMethod. Must be one of: {string.Join(", ", ValidPaymentMethods)}",
                ErrorCode.ValidationError);
        }

        // Check remaining balance
        var totalPaid = await paymentRepository.GetTotalPaymentsByOrderAsync(dto.OrderId, ct);
        var remainingBalance = order.TotalAmount - totalPaid;

        if (dto.Amount > remainingBalance)
        {
            logger.LogWarning("Payment exceeds remaining balance [{Amount}] [{RemainingBalance}]", dto.Amount,
                remainingBalance);
            return Response<PaymentDto>.Fail($"Payment amount exceeds remaining balance of {remainingBalance}",
                ErrorCode.ValidationError);
        }

        var payment = new Payment
        {
            CompanyId = companyId,
            OrderId = dto.OrderId,
            UserId = userId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            CollectedAt = DateTime.UtcNow,
            Lat = dto.Lat,
            Lng = dto.Lng
        };

        var created = await paymentRepository.AddAsync(payment, ct);

        logger.LogInformation("Payment recorded [{PaymentId}] [{OrderId}] [{Amount}]",
            created.Id, created.OrderId, created.Amount);

        var paymentWithRelations = await paymentRepository.GetByIdAsync(created.Id, ct);
        return Response<PaymentDto>.Ok(MapToDto(paymentWithRelations!));
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto(
            payment.Id,
            payment.CompanyId,
            payment.OrderId,
            payment.Order.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            payment.UserId,
            payment.User.FullName,
            payment.Amount,
            payment.PaymentMethod,
            payment.CollectedAt,
            payment.Lat,
            payment.Lng,
            payment.CreatedAt,
            payment.UpdatedAt
        );
    }
}
