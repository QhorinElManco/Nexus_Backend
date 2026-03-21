using FluentValidation;
using Nexus.Application.Dto.Roles;

namespace Nexus.Application.Validators.Roles;

public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters")
            .MaximumLength(50).WithMessage("Name must be at most 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must be at most 200 characters")
            .When(x => x.Description is not null);
    }
}
