using FluentValidation;
using Nexus.Application.Dto.Sales;

namespace Nexus.Application.Validators.Sales;

public class UpdateVisitDtoValidator : AbstractValidator<UpdateVisitDto>
{
    public UpdateVisitDtoValidator()
    {
        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status must be at most 50 characters");

        RuleFor(x => x.CancelReason)
            .MaximumLength(500).WithMessage("CancelReason must be at most 500 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must be at most 1000 characters");
    }
}
