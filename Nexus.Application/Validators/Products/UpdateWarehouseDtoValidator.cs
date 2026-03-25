using FluentValidation;
using Nexus.Application.Dto.Products;

namespace Nexus.Application.Validators.Products;

public class UpdateWarehouseDtoValidator : AbstractValidator<UpdateWarehouseDto>
{
    public UpdateWarehouseDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters");

        RuleFor(x => x.WarehouseTypeId)
            .GreaterThan(0).When(x => x.WarehouseTypeId.HasValue)
            .WithMessage("WarehouseTypeId must be greater than 0");

        RuleFor(x => x.ManagerId)
            .GreaterThan(0).When(x => x.ManagerId.HasValue)
            .WithMessage("ManagerId must be greater than 0");

        RuleFor(x => x.Lat)
            .InclusiveBetween(-90, 90).When(x => x.Lat.HasValue)
            .WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Lng)
            .InclusiveBetween(-180, 180).When(x => x.Lng.HasValue)
            .WithMessage("Longitude must be between -180 and 180");
    }
}