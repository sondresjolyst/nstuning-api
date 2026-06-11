using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace nstuning_api.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? UserId(this ClaimsPrincipal principal) =>
            principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }
}
