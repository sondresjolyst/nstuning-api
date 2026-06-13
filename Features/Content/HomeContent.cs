using System.Text.Json;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Models.Admin;

namespace nstuning_api.Features.Content
{
    /// <summary>Get (public) / replace (admin) the home page sections, stored as a JSON array blob.</summary>
    public static class HomeContent
    {
        private const string Empty = "[]";

        public static async Task<IResult> Get(ApplicationDbContext db, CancellationToken ct)
        {
            var settings = await db.AppSettings.FindAsync([1], ct);
            var json = string.IsNullOrWhiteSpace(settings?.HomePageJson) ? Empty : settings!.HomePageJson;
            return TypedResults.Content(json, "application/json");
        }

        public static async Task<IResult> Put(JsonElement sections, ApplicationDbContext db, CancellationToken ct)
        {
            if (sections.ValueKind != JsonValueKind.Array)
                return TypedResults.Problem("Expected a JSON array of sections.", statusCode: StatusCodes.Status400BadRequest);

            var json = sections.GetRawText();
            var settings = await db.AppSettings.FindAsync([1], ct);
            if (settings == null)
            {
                settings = new AppSettings { Id = 1, HomePageJson = json };
                db.AppSettings.Add(settings);
            }
            else
            {
                settings.HomePageJson = json;
            }

            await db.SaveChangesAsync(ct);
            return TypedResults.Content(json, "application/json");
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                app.MapGet("/api/content/home", Get).AllowAnonymous();
                app.MapPut("/api/content/home", Put).RequireAuthorization(Policies.Admin);
            }
        }
    }
}
