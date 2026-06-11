using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nstuning_api.Models
{
    public class DynoRun
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(160)]
        public required string Slug { get; set; }

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

        public string? CoverImageData { get; set; }

        [MaxLength(50)]
        public string? CoverImageContentType { get; set; }

        public bool Published { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DynoRunReport? Report { get; set; }
    }
}
