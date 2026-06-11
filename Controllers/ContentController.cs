using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using nstuning_api.Models;
using nstuning_api.Models.Admin;
using Swashbuckle.AspNetCore.Annotations;

namespace nstuning_api.Controllers
{
    /// <summary>
    /// Owner-editable site content (home page sections).
    /// </summary>
    [ApiController]
    [Route("api/content")]
    [EnableCors("AllowAllOrigins")]
    public class ContentController : ControllerBase
    {
        private const string Empty = "[]";
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContentController> _logger;

        public ContentController(ApplicationDbContext context, ILogger<ContentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Gets the home page sections (public). Returns an empty array when none configured.
        /// </summary>
        [HttpGet("home")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Gets the home page sections.")]
        public async Task<IActionResult> GetHome(CancellationToken ct = default)
        {
            var settings = await _context.AppSettings.FindAsync([1], ct);
            var json = string.IsNullOrWhiteSpace(settings?.HomePageJson) ? Empty : settings!.HomePageJson;
            return Content(json, "application/json");
        }

        /// <summary>
        /// Replaces the home page sections (admin only). Body is a JSON array of sections.
        /// </summary>
        [HttpPut("home")]
        [Authorize(Policy = "Admin")]
        [SwaggerOperation(Summary = "Replaces the home page sections.")]
        public async Task<IActionResult> PutHome([FromBody] JsonElement sections, CancellationToken ct = default)
        {
            if (sections.ValueKind != JsonValueKind.Array)
                return BadRequest(new { message = "Expected a JSON array of sections." });

            var json = sections.GetRawText();

            var settings = await _context.AppSettings.FindAsync([1], ct);
            if (settings == null)
            {
                settings = new AppSettings { Id = 1, HomePageJson = json };
                _context.AppSettings.Add(settings);
            }
            else
            {
                settings.HomePageJson = json;
            }

            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Home page content updated by {UserId}", User.Identity?.Name);
            return Content(json, "application/json");
        }
    }
}
