using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SjolystInnovation.AppKit.SiteSettings;

namespace nstuning_api.Models.Admin
{
    public class AppSettings : ICompanySettings, IBrandingSettings
    {
        public int Id { get; set; } = 1;

        [MaxLength(200)]
        [EmailAddress]
        public string ContactRecipientEmail { get; set; } = "sonyslyst@gmail.com";

        [MaxLength(100)]
        public string CompanyName { get; set; } = "NS Tuning";

        [MaxLength(200)]
        public string CompanyLegalName { get; set; } = "Nordmark Service";

        [MaxLength(50)]
        public string OrgNumber { get; set; } = "923 202 374";

        public bool VatRegistered { get; set; } = true;

        [MaxLength(200)]
        public string StreetAddress { get; set; } = "Håbakken 7";

        /// <summary>Postnummer. Kept apart from the street so structured data can expose it as its own field.</summary>
        [MaxLength(20)]
        public string PostalCode { get; set; } = "4355";

        /// <summary>Poststed.</summary>
        [MaxLength(100)]
        public string AddressLocality { get; set; } = "Kvernaland";

        /// <summary>Fylke. Optional and omitted from structured data while blank.</summary>
        [MaxLength(100)]
        public string AddressRegion { get; set; } = "";

        /// <summary>The address as one line, for display. Derived, never stored.</summary>
        [NotMapped]
        public string FormattedAddress => PostalAddress.Format(StreetAddress, PostalCode, AddressLocality);

        [MaxLength(200)]
        public string PublicEmail { get; set; } = "";

        [MaxLength(40)]
        public string PublicPhone { get; set; } = "";

        public string? LogoData { get; set; }

        [MaxLength(50)]
        public string? LogoContentType { get; set; }

        public string? IconData { get; set; }

        [MaxLength(50)]
        public string? IconContentType { get; set; }
    }
}
