using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Dtos.Contact
{
    public class ContactRequestDto
    {
        [Required]
        [MaxLength(120)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public required string Email { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(160)]
        public string? Car { get; set; }

        [Required]
        [MaxLength(4000)]
        public required string Message { get; set; }
    }
}
