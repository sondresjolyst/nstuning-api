using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Models.Admin;

namespace nstuning_api.Features.Content
{
    public record CompanyInfoResponse(string Name, string LegalName, string Address);

    /// <summary>Public company info (name, legal name, address) for SSR pages and structured data.</summary>
    public static class CompanyInfo
    {
        public static async Task<IResult> Get(ApplicationDbContext db, CancellationToken ct)
        {
            var settings = await db.AppSettings.FindAsync([1], ct) ?? new AppSettings();
            return TypedResults.Ok(new CompanyInfoResponse(settings.CompanyName, settings.CompanyLegalName, settings.Address));
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app) =>
                app.MapGet("/api/company", Get).AllowAnonymous();
        }
    }
}
