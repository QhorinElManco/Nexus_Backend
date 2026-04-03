using FluentValidation;
using Nexus.Application.Dto.Sales;

namespace Nexus.Application.Validators.Sales;

public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
{
    public UpdateOrderDtoValidator()
    {
        RuleFor(x => x.OrderType)
            .MaximumLength(50).WithMessage("OrderType must be at most 50 characters");

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status must be at most 50 characters");

        RuleFor(x => x.VisitId)
            .GreaterThan(0).When(x => x.VisitId.HasValue)
            .WithMessage("VisitId must be greater than 0");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).When(x => x.WarehouseId.HasValue)
            .WithMessage("WarehouseId must be greater than 0");
    }
}
