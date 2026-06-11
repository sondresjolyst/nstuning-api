using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using nstuning_api.Models;
using nstuning_api.Models.Admin;
using Swashbuckle.AspNetCore.Annotations;

namespace nstuning_api.Controllers
{
    /// <summary>
    /// Owner-editable branding: logo and icon.
    /// </summary>
    [ApiController]
    [Route("api/branding")]
    [EnableCors("AllowAllOrigins")]
    public class BrandingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BrandingController> _logger;

        public BrandingController(ApplicationDbContext context, ILogger<BrandingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current branding (public).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Gets the current branding.")]
        public async Task<IActionResult> Get(CancellationToken ct = default)
        {
            var settings = await _context.AppSettings.FindAsync([1], ct);
            return Ok(new
            {
                logoData = settings?.LogoData,
                logoContentType = settings?.LogoContentType,
                iconData = settings?.IconData,
                iconContentType = settings?.IconContentType,
            });
        }

        /// <summary>
        /// Uploads or replaces the logo and/or icon (admin only). multipart/form-data.
        /// </summary>
        [HttpPut]
        [Authorize(Policy = "Admin")]
        [SwaggerOperation(Summary = "Uploads or replaces the logo and/or icon.")]
        public async Task<IActionResult> Update(
            [FromForm] IFormFile? logo,
            [FromForm] IFormFile? icon,
            [FromForm] bool removeLogo = false,
            [FromForm] bool removeIcon = false,
            CancellationToken ct = default)
        {
            var settings = await _context.AppSettings.FindAsync([1], ct);
            if (settings == null)
            {
                settings = new AppSettings { Id = 1 };
                _context.AppSettings.Add(settings);
            }

            if (removeLogo)
            {
                settings.LogoData = null;
                settings.LogoContentType = null;
            }
            else if (logo != null)
            {
                (settings.LogoData, settings.LogoContentType) = await ReadImageAsync(logo, ct);
            }

            if (removeIcon)
            {
                settings.IconData = null;
                settings.IconContentType = null;
            }
            else if (icon != null)
            {
                (settings.IconData, settings.IconContentType) = await ReadImageAsync(icon, ct);
            }

            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Branding updated by {UserId}", User.Identity?.Name);
            return await Get(ct);
        }

        private static async Task<(string Data, string ContentType)> ReadImageAsync(IFormFile file, CancellationToken ct)
        {
            const long maxBytes = 2 * 1024 * 1024;
            if (file.Length == 0 || file.Length > maxBytes)
                throw new InvalidOperationException("Image must be between 1 byte and 2 MB.");
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("File must be an image.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            return (Convert.ToBase64String(ms.ToArray()), file.ContentType);
        }
    }
}
