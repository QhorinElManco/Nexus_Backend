using FluentValidation;
using Nexus.Application.Dto.Products;

namespace Nexus.Application.Validators.Products;

public class CreateWarehouseDtoValidator : AbstractValidator<CreateWarehouseDto>
{
    public CreateWarehouseDtoValidator()
    {
        RuleFor(x => x.ManagerId)
            .GreaterThan(0).WithMessage("ManagerId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters");

        RuleFor(x => x.WarehouseTypeId)
            .GreaterThan(0).WithMessage("WarehouseTypeId is required");

        RuleFor(x => x.Lat)
            .InclusiveBetween(-90, 90).When(x => x.Lat.HasValue)
            .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Lng)
            .InclusiveBetween(-180, 180).When(x => x.Lng.HasValue)
            .WithMessage("Longitude must be between -180 and 180");
    }
}
