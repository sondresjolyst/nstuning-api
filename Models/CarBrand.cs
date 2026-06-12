using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Models
{
    public class CarBrand
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
