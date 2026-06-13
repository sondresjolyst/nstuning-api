using nstuning_api.Features.Admin;

namespace nstuning_api.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message, string? replyTo = null);
        Task<EmailStatsDto> GetEmailStatsAsync(int days = 30);
    }
}
