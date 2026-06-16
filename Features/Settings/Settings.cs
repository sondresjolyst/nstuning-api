using FluentValidation;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Models.Admin;

namespace nstuning_api.Features.Settings
{
    public record SettingsBody(string ContactRecipientEmail, string CompanyName, string OrgNumber, bool VatRegistered, string Address);

    public class SettingsValidator : AbstractValidator<SettingsBody>
    {
        public SettingsValidator()
        {
            RuleFor(x => x.ContactRecipientEmail).NotEmpty().EmailAddress().MaximumLength(200);
            RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.OrgNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
        }
    }

    /// <summary>Get / update admin-managed application settings.</summary>
    public static class Settings
    {
        public static async Task<IResult> Get(ApplicationDbContext db, CancellationToken ct)
        {
            var settings = await db.AppSettings.FindAsync([1], ct) ?? new AppSettings();
            return TypedResults.Ok(new SettingsBody(settings.ContactRecipientEmail, settings.CompanyName, settings.OrgNumber, settings.VatRegistered, settings.Address));
        }

        public static async Task<IResult> Update(SettingsBody body, ApplicationDbContext db, CancellationToken ct)
        {
            var settings = await db.AppSettings.FindAsync([1], ct);
            if (settings == null)
            {
                settings = new AppSettings { Id = 1 };
                db.AppSettings.Add(settings);
            }

            settings.ContactRecipientEmail = body.ContactRecipientEmail;
            settings.CompanyName = body.CompanyName;
            settings.OrgNumber = body.OrgNumber;
            settings.VatRegistered = body.VatRegistered;
            settings.Address = body.Address;

            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new SettingsBody(settings.ContactRecipientEmail, settings.CompanyName, settings.OrgNumber, settings.VatRegistered, settings.Address));
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
