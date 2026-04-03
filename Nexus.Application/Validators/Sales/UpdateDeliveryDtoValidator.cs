using FluentValidation;
using Nexus.Application.Dto.Sales;

namespace Nexus.Application.Validators.Sales;

public class UpdateDeliveryDtoValidator : AbstractValidator<UpdateDeliveryDto>
{
    public UpdateDeliveryDtoValidator()
    {
        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status must be at most 50 characters");

        RuleFor(x => x.DeliveryLat)
            .InclusiveBetween(-90, 90).When(x => x.DeliveryLat.HasValue)
            .WithMessage("DeliveryLat must be between -90 and 90");

        RuleFor(x => x.DeliveryLng)
            .InclusiveBetween(-180, 180).When(x => x.DeliveryLng.HasValue)
            .WithMessage("DeliveryLng must be between -180 and 180");

        RuleFor(x => x.ProofOfDeliveryUrl)
            .MaximumLength(500).WithMessage("ProofOfDeliveryUrl must be at most 500 characters");
    }
}
