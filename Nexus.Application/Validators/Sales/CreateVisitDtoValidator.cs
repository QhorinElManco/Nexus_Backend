using FluentValidation;
using Nexus.Application.Dto.Sales;

namespace Nexus.Application.Validators.Sales;

public class CreateVisitDtoValidator : AbstractValidator<CreateVisitDto>
{
    public CreateVisitDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("CustomerId is required");

        RuleFor(x => x.CheckInLat)
            .InclusiveBetween(-90, 90).When(x => x.CheckInLat.HasValue)
            .WithMessage("CheckInLat must be between -90 and 90");

        RuleFor(x => x.CheckInLng)
            .InclusiveBetween(-180, 180).When(x => x.CheckInLng.HasValue)
            .WithMessage("CheckInLng must be between -180 and 180");

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status must be at most 50 characters");
    }
}
