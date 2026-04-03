using FluentValidation;
using Nexus.Application.Dto.Sales;

namespace Nexus.Application.Validators.Sales;

public class CreatePaymentDtoValidator : AbstractValidator<CreatePaymentDto>
{
    public CreatePaymentDtoValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("OrderId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("PaymentMethod is required")
            .MaximumLength(50).WithMessage("PaymentMethod must be at most 50 characters");

        RuleFor(x => x.Lat)
            .InclusiveBetween(-90, 90).When(x => x.Lat.HasValue)
            .WithMessage("Lat must be between -90 and 90");

        RuleFor(x => x.Lng)
            .InclusiveBetween(-180, 180).When(x => x.Lng.HasValue)
            .WithMessage("Lng must be between -180 and 180");
    }
}
