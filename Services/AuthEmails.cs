using nstuning_api.Models;
using nstuning_api.Models.Admin;
using SjolystInnovation.AppKit.Auth;

namespace nstuning_api.Services
{
    /// <summary>Sends the auth endpoints' emails with this app's templates.</summary>
    public class AuthEmails(ApplicationDbContext db, IEmailService email) : IAuthEmails<User>
    {
        public async Task SendPasswordResetAsync(User user, string code, CancellationToken cancellationToken)
        {
        var settings = await db.AppSettings.FindAsync([1], cancellationToken) ?? new AppSettings();
        await email.SendEmailAsync(user.Email!, "Password reset code",
            AuthEmailTemplates.PasswordReset(settings.CompanyName, user.FirstName, code));
        }
    }
}
