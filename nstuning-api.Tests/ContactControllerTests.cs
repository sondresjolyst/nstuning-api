using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using nstuning_api.Controllers;
using nstuning_api.Dtos.Contact;
using nstuning_api.Models.Admin;
using nstuning_api.Services;
using Xunit;

namespace nstuning_api.Tests;

public class ContactControllerTests : TestBase
{
    [Fact]
    public async Task Send_UsesConfiguredRecipientAndRepliesToSender()
    {
        await using var db = CreateDbContext();
        db.AppSettings.Add(new AppSettings { ContactRecipientEmail = "shop@nstuning.no" });
        await db.SaveChangesAsync();

        var email = new Mock<IEmailService>();
        var controller = new ContactController(db, email.Object, NullLogger<ContactController>.Instance);

        var dto = new ContactRequestDto { Name = "Ola", Email = "ola@kunde.no", Message = "Hi" };
        Assert.IsType<OkObjectResult>(await controller.Send(dto));

        email.Verify(e => e.SendEmailAsync(
            "shop@nstuning.no",
            It.IsAny<string>(),
            It.IsAny<string>(),
            "ola@kunde.no"), Times.Once);
    }

    [Fact]
    public async Task Send_FallsBackToDefaultRecipientWhenNoSettings()
    {
        await using var db = CreateDbContext();
        var email = new Mock<IEmailService>();
        var controller = new ContactController(db, email.Object, NullLogger<ContactController>.Instance);

        await controller.Send(new ContactRequestDto { Name = "Kari", Email = "kari@kunde.no", Message = "Hei" });

        email.Verify(e => e.SendEmailAsync(
            "sonyslyst@gmail.com", It.IsAny<string>(), It.IsAny<string>(), "kari@kunde.no"), Times.Once);
    }
}
