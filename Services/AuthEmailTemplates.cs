using System.Net;

namespace nstuning_api.Services
{
    public static class AuthEmailTemplates
    {
        public static string PasswordReset(string companyName, string? firstName, string code)
        {
            string Enc(string? v) => WebUtility.HtmlEncode(v ?? string.Empty);
            var greeting = string.IsNullOrWhiteSpace(firstName) ? "Hi there," : $"Hi {Enc(firstName)},";
            return $@"
<h2>Reset your password</h2>
<p>{greeting}</p>
<p>We received a request to reset your {Enc(companyName)} password. Use the code below to continue:</p>
<p style=""font-size:24px;font-weight:bold;letter-spacing:3px"">{Enc(code)}</p>
<p>This code expires in 30 minutes. If you didn't request a password reset, you can ignore this email — your password won't change.</p>";
        }
    }
}
