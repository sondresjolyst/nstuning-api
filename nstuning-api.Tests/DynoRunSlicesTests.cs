using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using nstuning_api.Features.DynoRuns;
using nstuning_api.Models;
using SjolystInnovation.AppKit.Storage;
using Xunit;

namespace nstuning_api.Tests;

public class DynoRunSlicesTests : TestBase
{
    private static DynoRun Run(string slug, bool published) => new() { Title = slug, Slug = slug, Published = published };
    private static HttpContext Http(bool isAdmin) => MakeControllerContext(isAdmin: isAdmin).HttpContext;

    [Fact]
    public async Task GetAll_Anonymous_ReturnsOnlyPublished()
    {
        await using var db = CreateDbContext();
        db.DynoRuns.AddRange(Run("a", true), Run("b", false), Run("c", true));
        await db.SaveChangesAsync();

        var result = await DynoRunQueries.GetAll(Http(false), db, RealMapper, default);
        var ok = Assert.IsType<Ok<IEnumerable<DynoRunDto>>>(result);
        Assert.Equal(2, ok.Value!.Count());
        Assert.All(ok.Value!, d => Assert.True(d.Published));
    }

    [Fact]
    public async Task GetAll_AdminWithAll_IncludesDrafts()
    {
        await using var db = CreateDbContext();
        db.DynoRuns.AddRange(Run("a", true), Run("b", false));
        await db.SaveChangesAsync();

        var result = await DynoRunQueries.GetAll(Http(true), db, RealMapper, default, true);
        var ok = Assert.IsType<Ok<IEnumerable<DynoRunDto>>>(result);
        Assert.Equal(2, ok.Value!.Count());
    }

    [Fact]
    public async Task GetBySlug_UnpublishedAnonymous_ReturnsNotFound()
    {
        await using var db = CreateDbContext();
        db.DynoRuns.Add(Run("draft", false));
        await db.SaveChangesAsync();

        Assert.IsType<NotFound>(await DynoRunQueries.GetBySlug("draft", Http(false), db, RealMapper, default));
    }

    [Fact]
    public async Task Create_GeneratesUniqueSlug()
    {
        await using var db = CreateDbContext();
        db.DynoRuns.Add(Run("volvo-242-turbo", true));
        await db.SaveChangesAsync();

        using var storage = new TestStorage(db);

        var dto = new CreateDynoRunDto { Title = "Volvo 242 Turbo", Published = true };
        var result = await DynoRunCommands.Create(dto, Http(true), db, storage.Reports, storage.Images, RealMapper, default);
        var created = Assert.IsType<Created<DynoRunDto>>(result);
        Assert.Equal("volvo-242-turbo-2", created.Value!.Slug);
    }

    [Fact]
    public async Task Create_WithReport_StoresFileAndSetsHasReport()
    {
        await using var db = CreateDbContext();
        using var storage = new TestStorage(db);

        var dto = new CreateDynoRunDto { Title = "Audi RS6", Published = true, Report = TestStorage.MakePdf("målinger.pdf") };
        var result = await DynoRunCommands.Create(dto, Http(true), db, storage.Reports, storage.Images, RealMapper, default);
        var created = Assert.IsType<Created<DynoRunDto>>(result);

        Assert.Single(storage.ReportFiles);
        Assert.True(created.Value!.HasReport);
        var report = Assert.Single(db.DynoRunReports);
        Assert.Equal("målinger.pdf", report.FileName);
        Assert.Equal("application/pdf", report.ContentType);
        Assert.Equal("user-1", report.UploadedByUserId);
    }

    [Fact]
    public async Task Create_WithNonPdfReport_IsRefusedAndStoresNothing()
    {
        await using var db = CreateDbContext();
        using var storage = new TestStorage(db);

        var dto = new CreateDynoRunDto { Title = "Golf", Report = TestStorage.MakeFile("<html></html>"u8.ToArray(), "report.pdf", "application/pdf", "Report") };
        await Assert.ThrowsAsync<AppKitFileRefusedException>(
            () => DynoRunCommands.Create(dto, Http(true), db, storage.Reports, storage.Images, RealMapper, default));

        Assert.Empty(db.DynoRuns);
        Assert.Empty(storage.ReportFiles);
    }

    [Fact]
    public async Task Update_WithCover_PersistsImageAndVariants()
    {
        await using var db = CreateDbContext();
        using var storage = new TestStorage(db);
        var created = Assert.IsType<Created<DynoRunDto>>(
            await DynoRunCommands.Create(new CreateDynoRunDto { Title = "Saab" }, Http(true), db, storage.Reports, storage.Images, RealMapper, default));

        var updated = Assert.IsType<Ok<DynoRunDto>>(
            await DynoRunCommands.Update(created.Value!.Id, new UpdateDynoRunDto { Title = "Saab", CoverImage = TestStorage.MakePng() },
                Http(true), db, storage.Reports, storage.Images, RealMapper, default));

        Assert.NotNull(updated.Value!.CoverImageId);
        var image = Assert.Single(db.ContentImages);
        Assert.Equal(updated.Value!.CoverImageId, image.Id);
        Assert.Equal((20, 10), (image.Width, image.Height));
        Assert.Equal("user-1", image.UploadedByUserId);
        Assert.NotEmpty(db.ContentImageVariants);
    }

    [Fact]
    public async Task Update_ReplacingCover_DeletesOldImageAndFiles()
    {
        await using var db = CreateDbContext();
        using var storage = new TestStorage(db);
        var created = Assert.IsType<Created<DynoRunDto>>(
            await DynoRunCommands.Create(new CreateDynoRunDto { Title = "Saab", CoverImage = TestStorage.MakePng() }, Http(true), db, storage.Reports, storage.Images, RealMapper, default));
        var oldId = created.Value!.CoverImageId;
        var filesPerImage = storage.ImageFiles.Length;

        var updated = Assert.IsType<Ok<DynoRunDto>>(
            await DynoRunCommands.Update(created.Value!.Id, new UpdateDynoRunDto { Title = "Saab 900", CoverImage = TestStorage.MakePng() },
                Http(true), db, storage.Reports, storage.Images, RealMapper, default));

        Assert.NotEqual(oldId, updated.Value!.CoverImageId);
        var image = Assert.Single(db.ContentImages);
        Assert.Equal(updated.Value!.CoverImageId, image.Id);
        Assert.Equal(filesPerImage, storage.ImageFiles.Length);
        Assert.Equal("Saab 900", (await db.DynoRuns.AsNoTracking().SingleAsync()).Title);
    }

    [Fact]
    public async Task Update_WithRefusedCover_ChangesNothing()
    {
        await using var db = CreateDbContext();
        using var storage = new TestStorage(db);
        var created = Assert.IsType<Created<DynoRunDto>>(
            await DynoRunCommands.Create(new CreateDynoRunDto { Title = "Volvo", Report = TestStorage.MakePdf() }, Http(true), db, storage.Reports, storage.Images, RealMapper, default));
        var reportPath = Assert.Single(storage.ReportFiles);

        var notAnImage = TestStorage.MakeFile("not an image"u8.ToArray(), "cover.png", "image/png", "CoverImage");
        await Assert.ThrowsAsync<AppKitFileRefusedException>(() => DynoRunCommands.Update(created.Value!.Id,
            new UpdateDynoRunDto { Title = "Volvo 940", Report = TestStorage.MakePdf(), CoverImage = notAnImage },
            Http(true), db, storage.Reports, storage.Images, RealMapper, default));

        Assert.Equal([reportPath], storage.ReportFiles);
        Assert.Empty(db.ContentImages);
        Assert.Equal("Volvo", (await db.DynoRuns.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken)).Title);
    }

    [Fact]
    public async Task Create_PersistsHubEngineFiguresAndDynoDate()
    {
        await using var db = CreateDbContext();
        var dto = new CreateDynoRunDto
        {
            Title = "Saab 99",
            DynoDate = new DateOnly(2026, 6, 12),
            HubPowerAfterWhp = 265,
            HubTorqueAfterWnm = 417,
            EnginePowerAfterHp = 315,
            EngineTorqueAfterNm = 494,
        };

        using var storage = new TestStorage(db);
        var created = Assert.IsType<Created<DynoRunDto>>(
            await DynoRunCommands.Create(dto, Http(true), db, storage.Reports, storage.Images, RealMapper, default));

        Assert.Equal(new DateOnly(2026, 6, 12), created.Value!.DynoDate);
        Assert.Equal(265, created.Value!.HubPowerAfterWhp);
        Assert.Equal(417, created.Value!.HubTorqueAfterWnm);
        Assert.Equal(315, created.Value!.EnginePowerAfterHp);
        Assert.Equal(494, created.Value!.EngineTorqueAfterNm);
    }

    [Fact]
    public async Task Update_ReuploadingReport_KeepsOriginalCreatedAt()
    {
        await using var db = CreateDbContext();
        using var storage = new TestStorage(db);

        var created = Assert.IsType<Created<DynoRunDto>>(
            await DynoRunCommands.Create(new CreateDynoRunDto { Title = "M5", Report = TestStorage.MakePdf() }, Http(true), db, storage.Reports, storage.Images, RealMapper, default));

        var report = db.DynoRunReports.First();
        var originalDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        report.CreatedAt = originalDate;
        await db.SaveChangesAsync();

        await DynoRunCommands.Update(created.Value!.Id, new UpdateDynoRunDto { Title = "M5", Report = TestStorage.MakePdf() }, Http(true), db, storage.Reports, storage.Images, RealMapper, default);

        Assert.Equal(originalDate, db.DynoRunReports.First().CreatedAt);
        Assert.Single(storage.ReportFiles);
    }

    [Fact]
    public async Task Delete_RemovesRunReportAndCover()
    {
        await using var db = CreateDbContext();
        using var storage = new TestStorage(db);

        var created = Assert.IsType<Created<DynoRunDto>>(
            await DynoRunCommands.Create(new CreateDynoRunDto { Title = "M3", Report = TestStorage.MakePdf(), CoverImage = TestStorage.MakePng() },
                Http(true), db, storage.Reports, storage.Images, RealMapper, default));
        Assert.NotEmpty(storage.ImageFiles);

        Assert.IsType<NoContent>(await DynoRunCommands.Delete(created.Value!.Id, db, storage.Reports, storage.Images, default));
        Assert.Empty(db.DynoRuns);
        Assert.Empty(db.ContentImages);
        Assert.Empty(db.ContentImageVariants);
        Assert.Empty(storage.ReportFiles);
        Assert.Empty(storage.ImageFiles);
    }

    [Fact]
    public async Task GetReport_Published_ServesPdfInlineWithItsName()
    {
        await using var db = CreateDbContext();
        using var storage = new TestStorage(db);
        var created = Assert.IsType<Created<DynoRunDto>>(
            await DynoRunCommands.Create(new CreateDynoRunDto { Title = "RS3", Published = true, Report = TestStorage.MakePdf("dyno ø.pdf") },
                Http(true), db, storage.Reports, storage.Images, RealMapper, default));

        var http = Http(false);
        http.RequestServices = storage.Services;
        http.Response.Body = new MemoryStream();
        var result = await DynoRunQueries.GetReport(created.Value!.Id, http, db, storage.Reports, default);
        await result.ExecuteAsync(http);

        Assert.Equal("application/pdf", http.Response.ContentType);
        var disposition = http.Response.Headers.ContentDisposition.ToString();
        Assert.StartsWith("inline", disposition);
        Assert.Contains("filename*=UTF-8''dyno%20%C3%B8.pdf", disposition);
        Assert.Equal("nosniff", http.Response.Headers.XContentTypeOptions.ToString());
    }

    [Fact]
    public async Task GetReport_Draft_AnonymousGetsNotFound()
    {
        await using var db = CreateDbContext();
        using var storage = new TestStorage(db);
        var created = Assert.IsType<Created<DynoRunDto>>(
            await DynoRunCommands.Create(new CreateDynoRunDto { Title = "RS4", Report = TestStorage.MakePdf() },
                Http(true), db, storage.Reports, storage.Images, RealMapper, default));

        Assert.IsType<NotFound>(await DynoRunQueries.GetReport(created.Value!.Id, Http(false), db, storage.Reports, default));
    }

    [Fact]
    public async Task GetReport_StoredPathOutsideTheStore_ReturnsNotFound()
    {
        await using var db = CreateDbContext();
        using var storage = new TestStorage(db);
        Directory.CreateDirectory(storage.Root);
        await File.WriteAllTextAsync(Path.Combine(storage.Root, "secret.pdf"), "%PDF-1.4 secret", TestContext.Current.CancellationToken);
        var run = Run("leak", true);
        run.Report = new DynoRunReport { FileName = "secret.pdf", ContentType = "application/pdf", StoredPath = "../secret.pdf" };
        db.DynoRuns.Add(run);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.IsType<NotFound>(await DynoRunQueries.GetReport(run.Id, Http(false), db, storage.Reports, default));
    }
}
