using FluentValidation;
using Nexus.Application.Dto.Suppliers;

namespace Nexus.Application.Validators.Suppliers;

public class UpdateSupplierDtoValidator : AbstractValidator<UpdateSupplierDto>
{
    public UpdateSupplierDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters");
    }
}
