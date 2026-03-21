using FluentValidation;
using Nexus.Application.Dto.Access;

namespace Nexus.Application.Validators.Access;

public class UpdateAccessDtoValidator : AbstractValidator<UpdateAccessDto>
{
    public UpdateAccessDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(50).WithMessage("Name must be at most 50 characters")
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Name can only contain letters, numbers, dots, underscores and hyphens");
    }
}
