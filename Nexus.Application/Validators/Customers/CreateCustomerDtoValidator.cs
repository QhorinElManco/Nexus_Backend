using FluentValidation;
using Nexus.Application.Dto.Customers;

namespace Nexus.Application.Validators.Customers;

public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerDtoValidator()
    {
        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("CompanyId must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters");

        RuleFor(x => x.TradeName)
            .MaximumLength(200).WithMessage("TradeName must be at most 200 characters");

        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage("TaxId is required")
            .MaximumLength(50).WithMessage("TaxId must be at most 50 characters");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .MaximumLength(50).WithMessage("Status must be at most 50 characters");

        RuleFor(x => x.Lat)
            .InclusiveBetween(-90, 90).When(x => x.Lat.HasValue)
            .WithMessage("Lat must be between -90 and 90");

        RuleFor(x => x.Lng)
            .InclusiveBetween(-180, 180).When(x => x.Lng.HasValue)
            .WithMessage("Lng must be between -180 and 180");
    }
}
