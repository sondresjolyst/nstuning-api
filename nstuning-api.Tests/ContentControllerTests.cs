using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using nstuning_api.Controllers;
using nstuning_api.Models;
using nstuning_api.Models.Admin;
using Xunit;

namespace nstuning_api.Tests;

public class ContentControllerTests : TestBase
{
    private static ContentController Create(ApplicationDbContext db) =>
        new(db, NullLogger<ContentController>.Instance)
        {
            ControllerContext = MakeControllerContext(isAdmin: true)
        };

    [Fact]
    public async Task GetHome_NoSettings_ReturnsEmptyArray()
    {
        await using var db = CreateDbContext();
        var result = Assert.IsType<ContentResult>(await Create(db).GetHome());
        Assert.Equal("[]", result.Content);
        Assert.Equal("application/json", result.ContentType);
    }

    [Fact]
    public async Task PutHome_PersistsSections()
    {
        await using var db = CreateDbContext();
        var sections = JsonSerializer.Deserialize<JsonElement>("""[{"id":"1","type":"hero","heading":"Hi"}]""");

        var result = Assert.IsType<ContentResult>(await Create(db).PutHome(sections));
        Assert.Contains("hero", result.Content);

        var stored = await db.AppSettings.FindAsync(1);
        Assert.Contains("Hi", stored!.HomePageJson);
    }

    [Fact]
    public async Task PutHome_RejectsNonArray()
    {
        await using var db = CreateDbContext();
        var notArray = JsonSerializer.Deserialize<JsonElement>("""{"foo":"bar"}""");
        Assert.IsType<BadRequestObjectResult>(await Create(db).PutHome(notArray));
    }
}
