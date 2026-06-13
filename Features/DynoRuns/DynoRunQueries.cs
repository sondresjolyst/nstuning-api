using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Services;

namespace nstuning_api.Features.DynoRuns
{
    /// <summary>Public read endpoints for dyno runs (list, by slug, report stream).</summary>
    public static class DynoRunQueries
    {
        private static bool IsAdmin(HttpContext http) => http.User.IsInRole(RoleNames.Admin);

        public static async Task<IResult> GetAll(HttpContext http, ApplicationDbContext db, IMapper mapper, CancellationToken ct, bool all = false)
        {
            var includeDrafts = all && IsAdmin(http);
            var query = db.DynoRuns.AsNoTracking().Include(d => d.Report).AsQueryable();
            if (!includeDrafts)
                query = query.Where(d => d.Published);

            var runs = await query.OrderBy(d => d.SortOrder).ThenByDescending(d => d.CreatedAt).ToListAsync(ct);
            return TypedResults.Ok(runs.Select(mapper.Map<DynoRunDto>));
        }

        public static async Task<IResult> GetBySlug(string slug, HttpContext http, ApplicationDbContext db, IMapper mapper, CancellationToken ct)
        {
            var run = await db.DynoRuns.AsNoTracking().Include(d => d.Report).FirstOrDefaultAsync(d => d.Slug == slug, ct);
            if (run == null || (!run.Published && !IsAdmin(http)))
                return TypedResults.NotFound();
            return TypedResults.Ok(mapper.Map<DynoRunDto>(run));
        }

        public static async Task<IResult> GetReport(int id, HttpContext http, ApplicationDbContext db, IReportStorageService storage, CancellationToken ct)
        {
            var run = await db.DynoRuns.AsNoTracking().Include(d => d.Report).FirstOrDefaultAsync(d => d.Id == id, ct);
            if (run == null || (!run.Published && !IsAdmin(http)) || run.Report == null)
                return TypedResults.NotFound();
            if (!storage.Exists(run.Report.StoredPath))
                return TypedResults.NotFound();

            http.Response.Headers.ContentDisposition = $"inline; filename=\"{run.Report.FileName}\"";
            return TypedResults.File(storage.OpenRead(run.Report.StoredPath), run.Report.ContentType);
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/dyno-runs").AllowAnonymous();
                group.MapGet("", GetAll);
                group.MapGet("{id:int}/report", GetReport);
                group.MapGet("{slug}", GetBySlug);
            }
        }
    }
}
