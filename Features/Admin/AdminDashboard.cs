using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Services;
using SjolystInnovation.AppKit.Storage;
using SjolystInnovation.AppKit.Storage.Images;

namespace nstuning_api.Features.Admin
{
    /// <summary>Admin dashboard: aggregate stats. The daily history is MapAppKitDailyStats, the roles MapAppKitUserAdmin and the email stats MapAppKitEmailStats.</summary>
    public static class AdminDashboard
    {
        public static async Task<IResult> GetStats(ApplicationDbContext db, IAppKitStorageUsage storageUsage, CancellationToken ct)
        {
            var usage = await storageUsage.GetAsync([AppKitContentImages.StoreName, ReportStore.Name], ct);

            return TypedResults.Ok(new AdminStatsDto
            {
                TotalUsers = await db.Users.CountAsync(u => !u.IsDeleted, ct),
                PublishedDynoRuns = await db.DynoRuns.CountAsync(d => d.Published, ct),
                DraftDynoRuns = await db.DynoRuns.CountAsync(d => !d.Published, ct),
                ContentImages = await db.ContentImages.CountAsync(ct),
                StorageUsedBytes = usage.StorageUsedBytes,
                DiskTotalBytes = usage.DiskTotalBytes,
                DiskFreeBytes = usage.DiskFreeBytes
            });
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/admin").RequireAuthorization(Policies.Admin);
                group.MapGet("stats", GetStats);
            }
        }
    }
}
