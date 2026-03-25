using FluentValidation;
using Nexus.Application.Dto.Suppliers;

namespace Nexus.Application.Validators.Suppliers;

public class CreateSupplierDtoValidator : AbstractValidator<CreateSupplierDto>
{
    public CreateSupplierDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters");

        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage("TaxId is required")
            .MaximumLength(50).WithMessage("TaxId must be at most 50 characters");
    }
}
