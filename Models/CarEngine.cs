using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Models
{
    public class CarEngine
    {
        [Key]
        public int Id { get; set; }

        public int VariantId { get; set; }
        public CarVariant? Variant { get; set; }

        [Required]
        [MaxLength(120)]
        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
