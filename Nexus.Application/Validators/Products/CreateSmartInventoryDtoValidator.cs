using FluentValidation;
using Nexus.Application.Dto.Products;

namespace Nexus.Application.Validators.Products;

public class CreateSmartInventoryDtoValidator : AbstractValidator<CreateSmartInventoryDto>
{
    public CreateSmartInventoryDtoValidator()
    {
        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).WithMessage("WarehouseId is required");

        RuleFor(x => x.SkuId)
            .GreaterThan(0).WithMessage("SkuId is required");

        RuleFor(x => x.SupplierId)
            .GreaterThan(0).WithMessage("SupplierId is required");

        RuleFor(x => x.LeadTimeDays)
            .GreaterThanOrEqualTo(0).WithMessage("LeadTimeDays must be >= 0");

        RuleFor(x => x.ReorderPoint)
            .GreaterThanOrEqualTo(0).WithMessage("ReorderPoint must be >= 0");

        RuleFor(x => x.TargetStock)
            .GreaterThan(0).WithMessage("TargetStock must be > 0");

        RuleFor(x => x.CoverageDays)
            .GreaterThanOrEqualTo(0).WithMessage("CoverageDays must be >= 0");
    }
}
