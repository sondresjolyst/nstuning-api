using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using nstuning_api.Features.Content;
using Xunit;

namespace nstuning_api.Tests;

public class ContentSliceTests : TestBase
{
    [Fact]
    public async Task GetHome_NoSettings_ReturnsEmptyArray()
    {
        await using var db = CreateDbContext();
        var content = Assert.IsType<ContentHttpResult>(await HomeContent.Get(db, default));
        Assert.Equal("[]", content.ResponseContent);
        Assert.Equal("application/json", content.ContentType);
    }

    [Fact]
    public async Task PutHome_PersistsSections()
    {
        await using var db = CreateDbContext();
        var sections = JsonSerializer.Deserialize<JsonElement>("""[{"id":"1","type":"hero","heading":"Hi"}]""");

        var content = Assert.IsType<ContentHttpResult>(await HomeContent.Put(sections, db, default));
        Assert.Contains("hero", content.ResponseContent!);

        var stored = await db.AppSettings.FindAsync(1);
        Assert.Contains("Hi", stored!.HomePageJson);
    }

    [Fact]
    public async Task PutHome_RejectsNonArray()
    {
        await using var db = CreateDbContext();
        var notArray = JsonSerializer.Deserialize<JsonElement>("""{"foo":"bar"}""");
        Assert.IsType<ProblemHttpResult>(await HomeContent.Put(notArray, db, default));
    }
}
