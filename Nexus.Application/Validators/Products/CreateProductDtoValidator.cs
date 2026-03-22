using FluentValidation;
using Nexus.Application.Dto.Products;

namespace Nexus.Application.Validators.Products;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("CompanyId must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters");

        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Brand must be at most 100 characters");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).When(x => x.CategoryId.HasValue)
            .WithMessage("CategoryId must be greater than 0");
    }
}