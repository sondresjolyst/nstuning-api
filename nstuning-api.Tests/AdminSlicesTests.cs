using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using nstuning_api.Features.Admin;
using nstuning_api.Models;
using nstuning_api.Models.Admin;
using nstuning_api.Services;
using SjolystInnovation.AppKit.AdminStats;
using SjolystInnovation.AppKit.Storage;
using SjolystInnovation.AppKit.Storage.Images;
using Xunit;

namespace nstuning_api.Tests;

public class AdminSlicesTests : TestBase
{
    private static DynoRun Run(string slug, bool published) => new() { Title = slug, Slug = slug, Published = published };

    [Fact]
    public async Task GetStats_ReturnsCounts()
    {
        await using var db = CreateDbContext();
        var deleted = MakeUser("u-del", "del@example.com");
        deleted.IsDeleted = true;
        db.Users.AddRange(MakeUser("u1", "a@example.com"), MakeUser("u2", "b@example.com"), deleted);
        db.DynoRuns.AddRange(Run("a", true), Run("b", true), Run("c", false));
        db.ContentImages.Add(new ContentImage { FileName = "x.png", ContentType = "image/png", StoredPath = "x.png" });
        await db.SaveChangesAsync();
        using var storage = new TestStorage(db);
        Directory.CreateDirectory(storage.ReportsPath);
        Directory.CreateDirectory(storage.ImagesPath);
        await File.WriteAllBytesAsync(Path.Combine(storage.ReportsPath, "a.pdf"), new byte[100], TestContext.Current.CancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(storage.ImagesPath, "a.png"), new byte[23], TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<AdminStatsDto>>(await AdminDashboard.GetStats(db,
            storage.Services.GetRequiredService<IAppKitStorageUsage>(), default));
        Assert.Equal(123, ok.Value!.StorageUsedBytes);
        Assert.True(ok.Value.DiskTotalBytes > 0);
        Assert.InRange(ok.Value.DiskFreeBytes, 1, ok.Value.DiskTotalBytes);
        Assert.Equal(2, ok.Value!.TotalUsers);
        Assert.Equal(2, ok.Value.PublishedDynoRuns);
        Assert.Equal(1, ok.Value.DraftDynoRuns);
        Assert.Equal(1, ok.Value.ContentImages);
    }

    [Fact]
    public async Task DailyStats_SampleAsync_CountsLiveTotals()
    {
        await using var db = CreateDbContext();
        var deleted = MakeUser("u-del", "del@example.com");
        deleted.IsDeleted = true;
        db.Users.AddRange(MakeUser("u1", "a@example.com"), deleted);
        db.DynoRuns.AddRange(Run("a", true), Run("b", false), Run("c", false));
        db.ContentImages.Add(new ContentImage { FileName = "x.png", ContentType = "image/png", StoredPath = "x.png" });
        await db.SaveChangesAsync();

        var sample = await DailyStats.SampleAsync(db, TestContext.Current.CancellationToken);

        Assert.Equal(1, sample.TotalUsers);
        Assert.Equal(1, sample.PublishedDynoRuns);
        Assert.Equal(2, sample.DraftDynoRuns);
        Assert.Equal(1, sample.ContentImages);
    }

    [Fact]
    public async Task DailyStats_History_EndsWithLiveTodayRow()
    {
        await using var db = CreateDbContext();
        db.Users.Add(MakeUser("u1", "a@example.com"));
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        db.DailyStatSnapshots.Add(new DailyStatSnapshot { Date = yesterday, TotalUsers = 7 });
        await db.SaveChangesAsync();
        var stats = new AppKitDailyStats<ApplicationDbContext, DailyStatSnapshot>(
            Options.Create(new AppKitDailyStatsOptions<ApplicationDbContext, DailyStatSnapshot> { LiveSampler = DailyStats.SampleAsync }),
            TimeProvider.System);

        var history = await stats.GetHistoryAsync(db, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal([yesterday, yesterday.AddDays(1)], history.Select(h => h.Date));
        Assert.Equal(7, history[0].TotalUsers);
        Assert.Equal(1, history[1].TotalUsers);
    }

    [Fact]
    public async Task GetStats_PassesTheContentImagesAndReportStoresAndCopiesTheUsage()
    {
        await using var db = CreateDbContext();
        var usage = new Mock<IAppKitStorageUsage>();
        usage.Setup(u => u.GetAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppKitStorageUsage(73400320, 10737418240, 8589934592));

        var ok = Assert.IsType<Ok<AdminStatsDto>>(await AdminDashboard.GetStats(db, usage.Object, default));

        usage.Verify(u => u.GetAsync(It.Is<IEnumerable<string>>(n => n.SequenceEqual(new[] { AppKitContentImages.StoreName, ReportStore.Name })), It.IsAny<CancellationToken>()));
        Assert.Equal(73400320, ok.Value!.StorageUsedBytes);
        Assert.Equal(10737418240, ok.Value.DiskTotalBytes);
        Assert.Equal(8589934592, ok.Value.DiskFreeBytes);
    }
}
