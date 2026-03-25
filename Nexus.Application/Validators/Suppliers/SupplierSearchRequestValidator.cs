using FluentValidation;
using Nexus.Application.Dto.Suppliers;

namespace Nexus.Application.Validators.Suppliers;

public class SupplierSearchRequestValidator : AbstractValidator<SupplierSearchRequest>
{
    public SupplierSearchRequestValidator()
    {
        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("CompanyId is required");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
    }
}
