using FluentValidation;
using Nexus.Application.Dto.Companies;

namespace Nexus.Application.Validators.Companies;

public class UpdateCompanyDtoValidator : AbstractValidator<UpdateCompanyDto>
{
    public UpdateCompanyDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters");
    }
}
