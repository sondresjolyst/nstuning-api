using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using nstuning_api.Dtos.Settings;
using nstuning_api.Models;
using nstuning_api.Models.Admin;
using Swashbuckle.AspNetCore.Annotations;

namespace nstuning_api.Controllers
{
    /// <summary>
    /// Admin-managed application settings.
    /// </summary>
    [ApiController]
    [Route("api/settings")]
    [EnableCors("AllowAllOrigins")]
    [Authorize(Policy = "Admin")]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ApplicationDbContext context, ILogger<SettingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current settings.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Gets the current settings.")]
        public async Task<IActionResult> Get(CancellationToken ct = default)
        {
            var settings = await _context.AppSettings.FindAsync([1], ct) ?? new AppSettings();
            return Ok(new SettingsDto { ContactRecipientEmail = settings.ContactRecipientEmail });
        }

        /// <summary>
        /// Updates the settings.
        /// </summary>
        [HttpPut]
        [SwaggerOperation(Summary = "Updates the settings.")]
        public async Task<IActionResult> Update([FromBody] SettingsDto dto, CancellationToken ct = default)
        {
            var settings = await _context.AppSettings.FindAsync([1], ct);
            if (settings == null)
            {
                settings = new AppSettings { Id = 1, ContactRecipientEmail = dto.ContactRecipientEmail };
                _context.AppSettings.Add(settings);
            }
            else
            {
                settings.ContactRecipientEmail = dto.ContactRecipientEmail;
            }

            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Settings updated by {UserId}", User.Identity?.Name);
            return Ok(new SettingsDto { ContactRecipientEmail = settings.ContactRecipientEmail });
        }
    }
}
