using FluentValidation;
using Nexus.Application.Dto.Products;

namespace Nexus.Application.Validators.Products;

public class KardexEntrySearchValidator : AbstractValidator<KardexEntrySearchRequest>
{
    private static readonly HashSet<string> ValidTransactionTypes =
        ["Sale", "Return", "Adjustment", "Purchase", "Transfer"];

    public KardexEntrySearchValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.TransactionType)
            .Must(t => t == null || ValidTransactionTypes.Contains(t))
            .WithMessage("Invalid TransactionType. Must be one of: Sale, Return, Adjustment, Purchase, Transfer");

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("DateFrom must be less than or equal to DateTo");
    }
}
