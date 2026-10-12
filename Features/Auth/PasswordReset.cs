using Microsoft.AspNetCore.Identity;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Models.Admin;
using nstuning_api.Services;
using SjolystInnovation.AppKit.Auth;

namespace nstuning_api.Features.Auth
{
    /// <summary>Code-based password reset: email a short code, verify it, set a new password.</summary>
    public static class PasswordReset
    {
        private const string RequestGeneric = "If this email is registered, a reset code has been sent.";
        private const string InvalidGeneric = "Invalid or expired reset code.";

        public static async Task<IResult> Request(RequestPasswordResetDto dto, ApplicationDbContext db, UserManager<User> users, IEmailService email, CancellationToken ct)
        {
            var user = await users.FindByEmailAsync(dto.Email);
            if (user != null && !user.IsDeleted)
            {
                var code = PasswordResetCodes.Issue(user);
                await users.UpdateAsync(user);

                var settings = await db.AppSettings.FindAsync([1], ct) ?? new AppSettings();
                await email.SendEmailAsync(user.Email!, "Password reset code",
                    AuthEmailTemplates.PasswordReset(settings.CompanyName, user.FirstName, code));
            }

            return TypedResults.Ok(new MessageResponse(RequestGeneric));
        }

        public static async Task<IResult> Reset(ResetPasswordDto dto, UserManager<User> users, IRefreshTokenService refreshTokens)
        {
            var user = await users.FindByEmailAsync(dto.Email);
            if (user == null || user.IsDeleted)
                return TypedResults.Problem(InvalidGeneric, statusCode: StatusCodes.Status400BadRequest);

            switch (PasswordResetCodes.Check(user, dto.Code))
            {
                case CodeCheck.TooManyAttempts:
                    return TypedResults.Problem("Too many attempts. Request a new reset code.", statusCode: StatusCodes.Status429TooManyRequests);
                case CodeCheck.Invalid:
                    await users.UpdateAsync(user);
                    return TypedResults.Problem(InvalidGeneric, statusCode: StatusCodes.Status400BadRequest);
            }

            var token = await users.GeneratePasswordResetTokenAsync(user);
            var result = await users.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!result.Succeeded)
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["NewPassword"] = result.Errors.Select(e => e.Description).ToArray()
                });

            PasswordResetCodes.Complete(user);
            await users.UpdateAsync(user);
            // A new password ends every session, so one that was taken over cannot carry on.
            await refreshTokens.RevokeAllForUserAsync(user.Id);

            return TypedResults.Ok(new MessageResponse("Password reset successfully."));
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                app.MapPost("/api/auth/request-password-reset", Request).AllowAnonymous().WithValidation<RequestPasswordResetDto>();
                app.MapPost("/api/auth/reset-password", Reset).AllowAnonymous().WithValidation<ResetPasswordDto>();
            }
        }
    }
}
