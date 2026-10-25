using nstuning_api.Models.Admin;
using nstuning_api.Services;
using Xunit;

namespace nstuning_api.Tests;

public class AuthEmailBrandTests : TestBase
{
    [Fact]
    public async Task GetBrandAsync_PassesTheRawOrgNumberAndVatFlag()
    {
        await using var db = CreateDbContext();
        db.AppSettings.Add(new AppSettings { CompanyName = "NS Tuning", CompanyLegalName = "Nordmark Service", OrgNumber = "923202374", VatRegistered = true, PublicEmail = "post@example.com" });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var brand = await new AuthEmailBrand(db).GetBrandAsync(TestContext.Current.CancellationToken);

        Assert.Equal("NS Tuning", brand.Name);
        Assert.Equal("Nordmark Service", brand.LegalName);
        Assert.Equal("923202374", brand.OrgNumber);
        Assert.True(brand.VatRegistered);
        Assert.Equal("Håbakken 7, 4355 Kvernaland", brand.Address);
        Assert.Equal("post@example.com", brand.Email);
    }

    [Fact]
    public async Task GetBrandAsync_WithoutVat_SaysNotRegistered()
    {
        await using var db = CreateDbContext();
        db.AppSettings.Add(new AppSettings { OrgNumber = "923 202 374", VatRegistered = false });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var brand = await new AuthEmailBrand(db).GetBrandAsync(TestContext.Current.CancellationToken);

        Assert.Equal("923 202 374", brand.OrgNumber);
        Assert.False(brand.VatRegistered);
    }
}
