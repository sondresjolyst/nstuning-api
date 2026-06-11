using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Dtos.Settings
{
    public class SettingsDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public required string ContactRecipientEmail { get; set; }
    }
}
