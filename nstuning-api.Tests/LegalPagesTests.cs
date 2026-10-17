using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using SjolystInnovation.AppKit.SiteSettings;
using Xunit;

namespace nstuning_api.Tests;

public class LegalPagesTests
{
    private static readonly string[] Keys = ["terms", "privacy", "cookies"];

    private static ServiceProvider Services()
    {
        var name = Guid.NewGuid().ToString();
        return new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(name))
            .BuildServiceProvider();
    }

    [Fact]
    public void LegalSeedText_CoversEveryPageInEnglishOnce()
    {
        Assert.Equal(Keys.Order(), LegalSeedText.All.Select(p => p.Key).Order());
        foreach (var page in LegalSeedText.All)
        {
            Assert.Equal("en", page.Locale);
            Assert.NotEmpty(page.Title);
            Assert.NotEmpty(page.BodyMarkdown);
        }
    }

    [Fact]
    public void LegalSeedText_NamesTheCompany()
    {
        var terms = LegalSeedText.All.Single(p => p.Key == "terms").BodyMarkdown;
        Assert.Contains("Nordmark Service AS", terms);
        Assert.Contains("923 202 374 MVA", terms);
        Assert.Contains("Håbakken 7, 4355 Kvernaland", terms);
    }

    [Fact]
    public async Task Seed_WritesEveryPage()
    {
        await using var services = Services();

        await services.SeedAppKitLegalPagesAsync<ApplicationDbContext>(LegalSeedText.All, TestContext.Current.CancellationToken);

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var pages = await db.Set<LegalPage>().ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(Keys.Order(), pages.Select(p => p.Key).Order());
        Assert.All(pages, p => Assert.Equal("en", p.Locale));
    }

    [Fact]
    public async Task Seed_KeepsEditedText()
    {
        await using var services = Services();
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Set<LegalPage>().Add(new LegalPage { Key = "terms", Locale = "en", Title = "My terms", BodyMarkdown = "Own text" });
            await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await services.SeedAppKitLegalPagesAsync<ApplicationDbContext>(LegalSeedText.All, TestContext.Current.CancellationToken);

        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var terms = await db.Set<LegalPage>().SingleAsync(p => p.Key == "terms", TestContext.Current.CancellationToken);
            Assert.Equal("Own text", terms.BodyMarkdown);
            Assert.Equal(3, await db.Set<LegalPage>().CountAsync(TestContext.Current.CancellationToken));
        }
    }
}
