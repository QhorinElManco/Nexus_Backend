using FluentValidation;
using Nexos.Application.Dto.Companies;

namespace Nexos.Application.Validator.Companies;

public class CreateCompanyDtoValidator : AbstractValidator<CreateCompanyDto>
{
    public CreateCompanyDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(80).WithMessage("Name must be at most 80 characters");

        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage("TaxId is required")
            .MaximumLength(50).WithMessage("TaxId must be at most 50 characters");
    }
}
