using Microsoft.EntityFrameworkCore;
using nstuning_api.Models;
using nstuning_api.Models.Admin;

namespace nstuning_api.Features.Admin
{
    /// <summary>The counts behind the admin history chart.</summary>
    public static class DailyStats
    {
        /// <summary>The live counts, which AppKit.AdminStats stores once per completed UTC day.</summary>
        public static async Task<DailyStatSnapshot> SampleAsync(ApplicationDbContext db, CancellationToken ct) => new()
        {
            TotalUsers = await db.Users.CountAsync(u => !u.IsDeleted, ct),
            PublishedDynoRuns = await db.DynoRuns.CountAsync(d => d.Published, ct),
            DraftDynoRuns = await db.DynoRuns.CountAsync(d => !d.Published, ct),
            ContentImages = await db.ContentImages.CountAsync(ct)
        };
    }
}
