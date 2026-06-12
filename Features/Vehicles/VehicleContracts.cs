using FluentValidation;

namespace nstuning_api.Features.Vehicles
{
    public record VehicleItem(int Id, string Name);
    public record VariantTree(int Id, string Name, List<VehicleItem> Engines);
    public record ModelTree(int Id, string Name, List<VariantTree> Variants);
    public record BrandTree(int Id, string Name, List<ModelTree> Models);

    public record VehicleName(string Name);

    public class VehicleNameValidator : AbstractValidator<VehicleName>
    {
        public VehicleNameValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
    }
}
