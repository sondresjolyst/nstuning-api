using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;

namespace nstuning_api.Features.Auth
{
    /// <summary>Registers a new user (generic response to avoid email enumeration).</summary>
    public static class Register
    {
        private const string Generic = "If the email is available, an account has been created.";

        public static async Task<IResult> Handle(RegisterUserDto dto, UserManager<User> users, IMapper mapper)
        {
            var existing = await users.FindByEmailAsync(dto.Email);
            if (existing != null) return TypedResults.Ok(new MessageResponse(Generic));

            var user = mapper.Map<User>(dto);
            var result = await users.CreateAsync(user, dto.Password);
            if (result.Succeeded)
            {
                await users.AddToRoleAsync(user, RoleNames.Default);
                return TypedResults.Ok(new MessageResponse(Generic));
            }

            var fieldErrors = result.Errors
                .GroupBy(e => ErrorField(e.Code))
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            return TypedResults.ValidationProblem(fieldErrors);
        }

        private static string ErrorField(string code) =>
            code.Contains("UserName", StringComparison.OrdinalIgnoreCase) ? "UserName"
            : code.Contains("Email", StringComparison.OrdinalIgnoreCase) ? "Email"
            : code.Contains("Password", StringComparison.OrdinalIgnoreCase) ? "Password"
            : "UserName";

        public class Endpoint : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app) =>
                app.MapPost("/api/auth/register", Handle).AllowAnonymous().WithValidation<RegisterUserDto>();
        }
    }
}
