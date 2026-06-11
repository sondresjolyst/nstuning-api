using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Models.Admin
{
    public class AppSettings
    {
        public int Id { get; set; } = 1;

        [MaxLength(200)]
        [EmailAddress]
        public string ContactRecipientEmail { get; set; } = "sonyslyst@gmail.com";

        [MaxLength(100)]
        public string CompanyName { get; set; } = "NS Tuning";

        [MaxLength(200)]
        public string CompanyLegalName { get; set; } = "Nordmark Service";

        public string? HomePageJson { get; set; }
    }
}
