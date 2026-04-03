using FluentValidation;
using Nexus.Application.Dto.Sales;

namespace Nexus.Application.Validators.Sales;

public class OrderSearchRequestValidator : AbstractValidator<OrderSearchRequest>
{
    public OrderSearchRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).When(x => x.CustomerId.HasValue)
            .WithMessage("CustomerId must be greater than 0");

        RuleFor(x => x.OrderType)
            .MaximumLength(50).WithMessage("OrderType must be at most 50 characters");

        RuleFor(x => x.Status)
            .MaximumLength(50).WithMessage("Status must be at most 50 characters");

        RuleFor(x => x.UserId)
            .GreaterThan(0).When(x => x.UserId.HasValue)
            .WithMessage("UserId must be greater than 0");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("StartDate must be less than or equal to EndDate");
    }
}
