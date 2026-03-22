using FluentValidation;
using Nexus.Application.Dto.Customers;

namespace Nexus.Application.Validators.Customers;

public class CustomerSearchRequestValidator : AbstractValidator<CustomerSearchRequest>
{
    public CustomerSearchRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).When(x => x.CompanyId.HasValue)
            .WithMessage("CompanyId must be greater than 0");
    }
}
