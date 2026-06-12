using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Models
{
    public class CarModel
    {
        [Key]
        public int Id { get; set; }

        public int BrandId { get; set; }
        public CarBrand? Brand { get; set; }

        [Required]
        [MaxLength(120)]
        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
