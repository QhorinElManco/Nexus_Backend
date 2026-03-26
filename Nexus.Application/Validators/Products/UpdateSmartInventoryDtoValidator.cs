using FluentValidation;
using Nexus.Application.Dto.Products;

namespace Nexus.Application.Validators.Products;

public class UpdateSmartInventoryDtoValidator : AbstractValidator<UpdateSmartInventoryDto>
{
    public UpdateSmartInventoryDtoValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0).When(x => x.SupplierId.HasValue)
            .WithMessage("SupplierId must be > 0");

        RuleFor(x => x.LeadTimeDays)
            .GreaterThanOrEqualTo(0).When(x => x.LeadTimeDays.HasValue)
            .WithMessage("LeadTimeDays must be >= 0");

        RuleFor(x => x.ReorderPoint)
            .GreaterThanOrEqualTo(0).When(x => x.ReorderPoint.HasValue)
            .WithMessage("ReorderPoint must be >= 0");

        RuleFor(x => x.TargetStock)
            .GreaterThan(0).When(x => x.TargetStock.HasValue)
            .WithMessage("TargetStock must be > 0");

        RuleFor(x => x.CoverageDays)
            .GreaterThanOrEqualTo(0).When(x => x.CoverageDays.HasValue)
            .WithMessage("CoverageDays must be >= 0");
    }
}
