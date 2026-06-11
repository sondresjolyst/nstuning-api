using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using nstuning_api.Dtos.Contact;
using nstuning_api.Models;
using nstuning_api.Models.Admin;
using nstuning_api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace nstuning_api.Controllers
{
    /// <summary>
    /// Public contact / booking enquiry endpoint.
    /// </summary>
    [ApiController]
    [Route("api/contact")]
    [EnableCors("AllowAllOrigins")]
    [AllowAnonymous]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<ContactController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Sends a contact / booking enquiry to the configured recipient.
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Sends a contact / booking enquiry.")]
        public async Task<IActionResult> Send([FromBody] ContactRequestDto dto, CancellationToken ct = default)
        {
            var settings = await _context.AppSettings.FindAsync([1], ct) ?? new AppSettings();

            var body = BuildBody(dto);
            await _emailService.SendEmailAsync(
                settings.ContactRecipientEmail,
                $"New enquiry from {dto.Name}",
                body,
                replyTo: dto.Email);

            _logger.LogInformation("Contact enquiry sent to {Recipient}", settings.ContactRecipientEmail);
            return Ok(new { message = "Thanks — we'll be in touch." });
        }

        private static string BuildBody(ContactRequestDto dto)
        {
            string Enc(string? v) => WebUtility.HtmlEncode(v ?? string.Empty);
            return $@"
<h2>New enquiry</h2>
<p><strong>Name:</strong> {Enc(dto.Name)}</p>
<p><strong>Email:</strong> {Enc(dto.Email)}</p>
<p><strong>Phone:</strong> {Enc(dto.Phone)}</p>
<p><strong>Car:</strong> {Enc(dto.Car)}</p>
<p><strong>Message:</strong></p>
<p>{Enc(dto.Message).Replace("\n", "<br/>")}</p>";
        }
    }
}
