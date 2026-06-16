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

        [MaxLength(120)]
        public string? Trim { get; set; }

        public int? Year { get; set; }

        [MaxLength(160)]
        public string? Engine { get; set; }

        [MaxLength(60)]
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

        [MaxLength(32)]
        public string? CoverImageId { get; set; }

        public ContentImage? CoverImage { get; set; }

        public bool Published { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DynoRunReport? Report { get; set; }
    }
}
