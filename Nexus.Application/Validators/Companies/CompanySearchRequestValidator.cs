using FluentValidation;
using Nexus.Application.Dto.Companies;

namespace Nexus.Application.Validators.Companies;

public class CompanySearchRequestValidator : AbstractValidator<CompanySearchRequest>
{
    public CompanySearchRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
    }
}
