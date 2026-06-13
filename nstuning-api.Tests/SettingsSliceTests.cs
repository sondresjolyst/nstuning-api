using Microsoft.AspNetCore.Http.HttpResults;
using nstuning_api.Features.Settings;
using nstuning_api.Models.Admin;
using Xunit;

namespace nstuning_api.Tests;

public class SettingsSliceTests : TestBase
{
    [Fact]
    public async Task Get_DefaultsWhenNoRow()
    {
        await using var db = CreateDbContext();
        var ok = Assert.IsType<Ok<SettingsBody>>(await Settings.Get(db, default));
        Assert.Equal("sonyslyst@gmail.com", ok.Value!.ContactRecipientEmail);
    }

    [Fact]
    public async Task Update_PersistsRecipient()
    {
        await using var db = CreateDbContext();
        db.AppSettings.Add(new AppSettings());
        await db.SaveChangesAsync();

        var ok = Assert.IsType<Ok<SettingsBody>>(await Settings.Update(new SettingsBody("shop@nstuning.no", "New St 1, 0001 Oslo"), db, default));
        Assert.Equal("shop@nstuning.no", ok.Value!.ContactRecipientEmail);
        Assert.Equal("New St 1, 0001 Oslo", ok.Value!.Address);

        var stored = await db.AppSettings.FindAsync(1);
        Assert.Equal("shop@nstuning.no", stored!.ContactRecipientEmail);
        Assert.Equal("New St 1, 0001 Oslo", stored!.Address);
    }
}
