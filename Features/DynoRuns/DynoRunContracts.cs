using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace nstuning_api.Features.DynoRuns
{
    public class CreateDynoRunDto
    {
        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(80)] public string? CarMake { get; set; }
        [MaxLength(120)] public string? CarModel { get; set; }
        [MaxLength(120)] public string? Trim { get; set; }
        public int? Year { get; set; }
        [MaxLength(160)] public string? Engine { get; set; }
        [MaxLength(60)] public string? FuelType { get; set; }
        public DateOnly? DynoDate { get; set; }

        public int? HubPowerBeforeWhp { get; set; }
        public int? HubPowerAfterWhp { get; set; }
        public int? HubTorqueBeforeWnm { get; set; }
        public int? HubTorqueAfterWnm { get; set; }

        public int? EnginePowerBeforeHp { get; set; }
        public int? EnginePowerAfterHp { get; set; }
        public int? EngineTorqueBeforeNm { get; set; }
        public int? EngineTorqueAfterNm { get; set; }

        public string? Description { get; set; }

        public bool Published { get; set; }
        public int SortOrder { get; set; }

        public IFormFile? Report { get; set; }
        public IFormFile? CoverImage { get; set; }
    }

    public class UpdateDynoRunDto : CreateDynoRunDto { }

    public class DynoRunDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? CarMake { get; set; }
        public string? CarModel { get; set; }
        public string? Trim { get; set; }
        public int? Year { get; set; }
        public string? Engine { get; set; }
        public string? FuelType { get; set; }
        public DateOnly? DynoDate { get; set; }
        public int? HubPowerBeforeWhp { get; set; }
        public int? HubPowerAfterWhp { get; set; }
        public int? HubTorqueBeforeWnm { get; set; }
        public int? HubTorqueAfterWnm { get; set; }
        public int? EnginePowerBeforeHp { get; set; }
        public int? EnginePowerAfterHp { get; set; }
        public int? EngineTorqueBeforeNm { get; set; }
        public int? EngineTorqueAfterNm { get; set; }
        public string? Description { get; set; }
        public string? CoverImageId { get; set; }
        public bool Published { get; set; }
        public int SortOrder { get; set; }
        public bool HasReport { get; set; }
        public string? ReportFileName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateDynoRunValidator : AbstractValidator<CreateDynoRunDto>
    {
        public CreateDynoRunValidator() => RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }

    public class UpdateDynoRunValidator : AbstractValidator<UpdateDynoRunDto>
    {
        public UpdateDynoRunValidator() => RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
