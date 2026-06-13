using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Models
{
    public class CarVariant
    {
        [Key]
        public int Id { get; set; }

        public int ModelId { get; set; }
        public CarModel? Model { get; set; }

        [Required]
        [MaxLength(120)]
        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
