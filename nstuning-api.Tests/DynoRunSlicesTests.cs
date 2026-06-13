using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using nstuning_api.Features.DynoRuns;
using nstuning_api.Models;
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

        var dto = new CreateDynoRunDto { Title = "Volvo 242 Turbo", Published = true };
        var result = await DynoRunCommands.Create(dto, Http(true), db, new FakeReportStorage(), new FakeImageStorage(), RealMapper, default);
        var created = Assert.IsType<Created<DynoRunDto>>(result);
        Assert.Equal("volvo-242-turbo-2", created.Value!.Slug);
    }

    [Fact]
    public async Task Create_WithReport_StoresFileAndSetsHasReport()
    {
        await using var db = CreateDbContext();
        var storage = new FakeReportStorage();

        var dto = new CreateDynoRunDto { Title = "Audi RS6", Published = true, Report = FakeReportStorage.MakePdf() };
        var result = await DynoRunCommands.Create(dto, Http(true), db, storage, new FakeImageStorage(), RealMapper, default);
        var created = Assert.IsType<Created<DynoRunDto>>(result);

        Assert.Equal(1, storage.SaveCount);
        Assert.True(created.Value!.HasReport);
    }

    [Fact]
    public async Task Delete_RemovesRunAndReportFile()
    {
        await using var db = CreateDbContext();
        var storage = new FakeReportStorage();

        var created = Assert.IsType<Created<DynoRunDto>>(
            await DynoRunCommands.Create(new CreateDynoRunDto { Title = "M3", Report = FakeReportStorage.MakePdf() }, Http(true), db, storage, new FakeImageStorage(), RealMapper, default));
        var id = created.Value!.Id;

        Assert.IsType<NoContent>(await DynoRunCommands.Delete(id, db, storage, new FakeImageStorage(), default));
        Assert.Empty(db.DynoRuns);
        Assert.Equal(1, storage.DeleteCount);
    }
}
