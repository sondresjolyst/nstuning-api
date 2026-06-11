using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Dtos.DynoRun
{
    public class CreateDynoRunDto
    {
        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(80)]
        public string? CarMake { get; set; }

        [MaxLength(120)]
        public string? CarModel { get; set; }

        public int? Year { get; set; }

        [MaxLength(160)]
        public string? Engine { get; set; }

        [MaxLength(60)]
        public string? FuelType { get; set; }

        public int? PowerBeforeHp { get; set; }
        public int? PowerAfterHp { get; set; }
        public int? TorqueBeforeNm { get; set; }
        public int? TorqueAfterNm { get; set; }

        public string? Description { get; set; }

        public bool Published { get; set; }
        public int SortOrder { get; set; }

        public IFormFile? Report { get; set; }
        public IFormFile? CoverImage { get; set; }
    }

    public class UpdateDynoRunDto : CreateDynoRunDto
    {
    }

    public class DynoRunDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? CarMake { get; set; }
        public string? CarModel { get; set; }
        public int? Year { get; set; }
        public string? Engine { get; set; }
        public string? FuelType { get; set; }
        public int? PowerBeforeHp { get; set; }
        public int? PowerAfterHp { get; set; }
        public int? TorqueBeforeNm { get; set; }
        public int? TorqueAfterNm { get; set; }
        public string? Description { get; set; }
        public string? CoverImageData { get; set; }
        public string? CoverImageContentType { get; set; }
        public bool Published { get; set; }
        public int SortOrder { get; set; }
        public bool HasReport { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
