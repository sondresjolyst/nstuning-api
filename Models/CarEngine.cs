using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Models
{
    public class CarEngine
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Optional brand tag for grouping in the picker. Null = global. Any engine is pickable for any car (swaps).</summary>
        public int? BrandId { get; set; }
        public CarBrand? Brand { get; set; }

        [Required]
        [MaxLength(120)]
        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
