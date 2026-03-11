using FluentValidation;
using Nexos.Application.Dto.Companies;

namespace Nexos.Application.UseCases.Validators.Companies;

public class UpdateCompanyDtoValidator : AbstractValidator<UpdateCompanyDto>
{
    public UpdateCompanyDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(80).WithMessage("Name must be at most 80 characters");
    }
}
