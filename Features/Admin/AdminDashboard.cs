using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Services;
using SjolystInnovation.AppKit.Storage;
using SjolystInnovation.AppKit.Storage.Images;

namespace nstuning_api.Features.Admin
{
    /// <summary>Admin dashboard: aggregate stats, roles, Brevo email stats. The daily history is MapAppKitDailyStats.</summary>
    public static class AdminDashboard
    {
        public static async Task<IResult> GetStats(ApplicationDbContext db, IOptions<AppKitContentImagesOptions> images,
            IOptionsMonitor<AppKitFileStoreOptions> stores, ILoggerFactory loggerFactory, CancellationToken ct)
        {
            var imagesPath = Path.GetFullPath(images.Value.ImagesPath);
            var reportsPath = Path.GetFullPath(stores.Get(ReportStore.Name).Path);

            long diskTotal = 0, diskFree = 0;
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(imagesPath) ?? imagesPath);
                diskTotal = drive.TotalSize;
                diskFree = drive.AvailableFreeSpace;
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger("Admin").LogWarning("Disk info unavailable: {Error}", ex.Message);
            }

            return TypedResults.Ok(new AdminStatsDto
            {
                TotalUsers = await db.Users.CountAsync(u => !u.IsDeleted, ct),
                PublishedDynoRuns = await db.DynoRuns.CountAsync(d => d.Published, ct),
                DraftDynoRuns = await db.DynoRuns.CountAsync(d => !d.Published, ct),
                ContentImages = await db.ContentImages.CountAsync(ct),
                StorageUsedBytes = DirectorySize(imagesPath) + DirectorySize(reportsPath),
                DiskTotalBytes = diskTotal,
                DiskFreeBytes = diskFree
            });
        }

        public static IResult GetRoles() => TypedResults.Ok(RoleNames.AllRoles);

        public static async Task<IResult> GetEmailStats(IEmailService email, ILoggerFactory loggerFactory, int days = 30)
        {
            try
            {
                return TypedResults.Ok(await email.GetEmailStatsAsync(days));
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger("Admin").LogError("GetEmailStats failed: {Error}", ex.Message);
                return TypedResults.Problem("Failed to fetch email stats from Brevo.", statusCode: StatusCodes.Status502BadGateway);
            }
        }

        private static long DirectorySize(string path)
        {
            if (!Directory.Exists(path)) return 0;
            return new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/admin").RequireAuthorization(Policies.Admin);
                group.MapGet("stats", GetStats);
                group.MapGet("roles", GetRoles);
                group.MapGet("email-stats", GetEmailStats);
            }
        }
    }
}
