using FluentValidation;
using Nexus.Application.Dto.Sales;

namespace Nexus.Application.Validators.Sales;

public class PaymentSearchRequestValidator : AbstractValidator<PaymentSearchRequest>
{
    public PaymentSearchRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.OrderId)
            .GreaterThan(0).When(x => x.OrderId.HasValue)
            .WithMessage("OrderId must be greater than 0");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).When(x => x.CompanyId.HasValue)
            .WithMessage("CompanyId must be greater than 0");

        RuleFor(x => x.PaymentMethod)
            .MaximumLength(50).WithMessage("PaymentMethod must be at most 50 characters");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("StartDate must be less than or equal to EndDate");
    }
}
