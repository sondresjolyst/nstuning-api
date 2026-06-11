using Microsoft.AspNetCore.Mvc;
using nstuning_api.Dtos.DynoRun;
using nstuning_api.Models;
using Xunit;

namespace nstuning_api.Tests;

public class DynoRunsControllerTests : TestBase
{
    private static DynoRun Run(string slug, bool published) => new()
    {
        Title = slug,
        Slug = slug,
        Published = published
    };

    [Fact]
    public async Task GetAll_Anonymous_ReturnsOnlyPublished()
    {
        await using var db = CreateDbContext();
        db.DynoRuns.AddRange(Run("a", true), Run("b", false), Run("c", true));
        await db.SaveChangesAsync();

        var controller = CreateDynoRunsController(db, new FakeReportStorage(), isAdmin: false);
        var result = Assert.IsType<OkObjectResult>(await controller.GetAll());
        var list = Assert.IsAssignableFrom<IEnumerable<DynoRunDto>>(result.Value);

        Assert.Equal(2, list.Count());
        Assert.All(list, d => Assert.True(d.Published));
    }

    [Fact]
    public async Task GetAll_AdminWithAll_IncludesDrafts()
    {
        await using var db = CreateDbContext();
        db.DynoRuns.AddRange(Run("a", true), Run("b", false));
        await db.SaveChangesAsync();

        var controller = CreateDynoRunsController(db, new FakeReportStorage(), isAdmin: true);
        var result = Assert.IsType<OkObjectResult>(await controller.GetAll(all: true));
        var list = Assert.IsAssignableFrom<IEnumerable<DynoRunDto>>(result.Value);

        Assert.Equal(2, list.Count());
    }

    [Fact]
    public async Task GetBySlug_UnpublishedAnonymous_ReturnsNotFound()
    {
        await using var db = CreateDbContext();
        db.DynoRuns.Add(Run("draft", false));
        await db.SaveChangesAsync();

        var controller = CreateDynoRunsController(db, new FakeReportStorage(), isAdmin: false);
        Assert.IsType<NotFoundResult>(await controller.GetBySlug("draft"));
    }

    [Fact]
    public async Task Create_GeneratesUniqueSlug()
    {
        await using var db = CreateDbContext();
        db.DynoRuns.Add(Run("golf-r", true));
        await db.SaveChangesAsync();

        var controller = CreateDynoRunsController(db, new FakeReportStorage(), isAdmin: true);
        var dto = new CreateDynoRunDto { Title = "Golf R", Published = true };
        var created = Assert.IsType<CreatedAtActionResult>(await controller.Create(dto));
        var body = Assert.IsType<DynoRunDto>(created.Value);

        Assert.Equal("golf-r-2", body.Slug);
    }

    [Fact]
    public async Task Create_WithReport_StoresFileAndSetsHasReport()
    {
        await using var db = CreateDbContext();
        var storage = new FakeReportStorage();
        var controller = CreateDynoRunsController(db, storage, isAdmin: true);

        var dto = new CreateDynoRunDto { Title = "Audi RS6", Published = true, Report = FakeReportStorage.MakePdf() };
        var created = Assert.IsType<CreatedAtActionResult>(await controller.Create(dto));
        var body = Assert.IsType<DynoRunDto>(created.Value);

        Assert.Equal(1, storage.SaveCount);
        Assert.True(body.HasReport);
    }

    [Fact]
    public async Task Delete_RemovesRunAndReportFile()
    {
        await using var db = CreateDbContext();
        var storage = new FakeReportStorage();
        var controller = CreateDynoRunsController(db, storage, isAdmin: true);

        var created = Assert.IsType<CreatedAtActionResult>(
            await controller.Create(new CreateDynoRunDto { Title = "M3", Report = FakeReportStorage.MakePdf() }));
        var id = Assert.IsType<DynoRunDto>(created.Value).Id;

        Assert.IsType<NoContentResult>(await controller.Delete(id));
        Assert.Empty(db.DynoRuns);
        Assert.Equal(1, storage.DeleteCount);
    }
}
