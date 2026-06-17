using FluentValidation;

namespace nstuning_api.Features.Vehicles
{
    public record VehicleItem(int Id, string Name);
    public record VariantItem(int Id, string Name);
    public record ModelTree(int Id, string Name, string? Family, List<VariantItem> Variants, List<int> EngineIds);
    public record BrandTree(int Id, string Name, List<ModelTree> Models);
    public record EngineCatalogItem(int Id, string Name, int? BrandId);
    public record VehicleTreeResponse(List<BrandTree> Brands, List<EngineCatalogItem> Engines);

    public record VehicleName(string Name);
    public record EngineInput(string Name, int? BrandId);
    public record ModelFamily(string? Family);

    public class VehicleNameValidator : AbstractValidator<VehicleName>
    {
        public VehicleNameValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
    }

    public class EngineInputValidator : AbstractValidator<EngineInput>
    {
        public EngineInputValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
    }

    public class ModelFamilyValidator : AbstractValidator<ModelFamily>
    {
        public ModelFamilyValidator() => RuleFor(x => x.Family).MaximumLength(80);
    }
}
