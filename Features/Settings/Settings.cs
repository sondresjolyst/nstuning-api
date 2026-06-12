using FluentValidation;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Models.Admin;

namespace nstuning_api.Features.Settings
{
    public record SettingsBody(string ContactRecipientEmail);

    public class SettingsValidator : AbstractValidator<SettingsBody>
    {
        public SettingsValidator() =>
            RuleFor(x => x.ContactRecipientEmail).NotEmpty().EmailAddress().MaximumLength(200);
    }

    /// <summary>Get / update admin-managed application settings.</summary>
    public static class Settings
    {
        public static async Task<IResult> Get(ApplicationDbContext db, CancellationToken ct)
        {
            var settings = await db.AppSettings.FindAsync([1], ct) ?? new AppSettings();
            return TypedResults.Ok(new SettingsBody(settings.ContactRecipientEmail));
        }

        public static async Task<IResult> Update(SettingsBody body, ApplicationDbContext db, CancellationToken ct)
        {
            var settings = await db.AppSettings.FindAsync([1], ct);
            if (settings == null)
            {
                settings = new AppSettings { Id = 1, ContactRecipientEmail = body.ContactRecipientEmail };
                db.AppSettings.Add(settings);
            }
            else
            {
                settings.ContactRecipientEmail = body.ContactRecipientEmail;
            }

            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new SettingsBody(settings.ContactRecipientEmail));
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/settings").RequireAuthorization(Policies.Admin);
                group.MapGet("", Get);
                group.MapPut("", Update).WithValidation<SettingsBody>();
            }
        }
    }
}
