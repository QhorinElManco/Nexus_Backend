using FluentValidation;
using Nexus.Application.Dto.Sales;

namespace Nexus.Application.Validators.Sales;

public class CreateDeliveryDtoValidator : AbstractValidator<CreateDeliveryDto>
{
    public CreateDeliveryDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("OrderId is required");

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status must be at most 50 characters");
    }
}
