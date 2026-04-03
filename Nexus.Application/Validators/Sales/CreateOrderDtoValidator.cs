using FluentValidation;
using Nexus.Application.Dto.Sales;

namespace Nexus.Application.Validators.Sales;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("CustomerId is required");

        RuleFor(x => x.OrderType)
            .NotEmpty().WithMessage("OrderType is required")
            .MaximumLength(50).WithMessage("OrderType must be at most 50 characters");

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status must be at most 50 characters");

        RuleFor(x => x.VisitId)
            .GreaterThan(0).When(x => x.VisitId.HasValue)
            .WithMessage("VisitId must be greater than 0");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).When(x => x.WarehouseId.HasValue)
            .WithMessage("WarehouseId must be greater than 0");

        RuleFor(x => x.OrderDetails)
            .NotNull().When(x => x.OrderDetails != null && x.OrderDetails.Count > 0)
            .WithMessage("OrderDetails cannot be empty if provided");

        RuleForEach(x => x.OrderDetails).SetValidator(new CreateOrderDetailDtoValidator());
    }
}

public class CreateOrderDetailDtoValidator : AbstractValidator<CreateOrderDetailDto>
{
    public CreateOrderDetailDtoValidator()
    {
        RuleFor(x => x.SkuId)
            .GreaterThan(0).WithMessage("SkuId is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("UnitPrice must be greater than or equal to 0");
    }
}
