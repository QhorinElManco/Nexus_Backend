using FluentValidation;
using Nexus.Application.Dto.Products;

namespace Nexus.Application.Validators.Products;

public class CreateSkuDtoValidator : AbstractValidator<CreateSkuDto>
{
    public CreateSkuDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId must be greater than 0");

        RuleFor(x => x.Barcode)
            .NotEmpty().WithMessage("Barcode is required")
            .MaximumLength(50).WithMessage("Barcode must be at most 50 characters");

        RuleFor(x => x.UnitMeasure)
            .NotEmpty().WithMessage("UnitMeasure is required")
            .MaximumLength(50).WithMessage("UnitMeasure must be at most 50 characters");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("BasePrice must be greater than or equal to 0");
    }
}