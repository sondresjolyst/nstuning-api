using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using nstuning_api.Controllers;
using nstuning_api.Dtos.Settings;
using nstuning_api.Models;
using nstuning_api.Models.Admin;
using Xunit;

namespace nstuning_api.Tests;

public class SettingsControllerTests : TestBase
{
    private static SettingsController Create(ApplicationDbContext db) =>
        new(db, NullLogger<SettingsController>.Instance)
        {
            ControllerContext = MakeControllerContext(isAdmin: true)
        };

    [Fact]
    public async Task Get_DefaultsWhenNoRow()
    {
        await using var db = CreateDbContext();
        var result = Assert.IsType<OkObjectResult>(await Create(db).Get());
        var dto = Assert.IsType<SettingsDto>(result.Value);
        Assert.Equal("sonyslyst@gmail.com", dto.ContactRecipientEmail);
    }

    [Fact]
    public async Task Update_PersistsRecipient()
    {
        await using var db = CreateDbContext();
        db.AppSettings.Add(new AppSettings());
        await db.SaveChangesAsync();

        var controller = Create(db);
        var result = Assert.IsType<OkObjectResult>(
            await controller.Update(new SettingsDto { ContactRecipientEmail = "shop@nstuning.no" }));

        var dto = Assert.IsType<SettingsDto>(result.Value);
        Assert.Equal("shop@nstuning.no", dto.ContactRecipientEmail);

        var stored = await db.AppSettings.FindAsync(1);
        Assert.Equal("shop@nstuning.no", stored!.ContactRecipientEmail);
    }
}
