using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using nstuning_api.Features.Contact;
using nstuning_api.Infrastructure;
using nstuning_api.Models.Admin;
using nstuning_api.Services;
using Xunit;

namespace nstuning_api.Tests;

public class ContactSliceTests : TestBase
{
    [Fact]
    public async Task Send_UsesConfiguredRecipientAndRepliesToSender()
    {
        await using var db = CreateDbContext();
        db.AppSettings.Add(new AppSettings { ContactRecipientEmail = "shop@nstuning.no" });
        await db.SaveChangesAsync();
        var email = new Mock<IEmailService>();

        var req = new ContactRequest("Ola", "ola@kunde.no", null, null, "Hi");
        Assert.IsType<Ok<MessageResponse>>(await SendEnquiry.Handle(req, db, email.Object, default));

        email.Verify(e => e.SendEmailAsync("shop@nstuning.no", It.IsAny<string>(), It.IsAny<string>(), "ola@kunde.no"), Times.Once);
    }

    [Fact]
    public async Task Send_FallsBackToDefaultRecipientWhenNoSettings()
    {
        await using var db = CreateDbContext();
        var email = new Mock<IEmailService>();

        await SendEnquiry.Handle(new ContactRequest("Kari", "kari@kunde.no", null, null, "Hei"), db, email.Object, default);

        email.Verify(e => e.SendEmailAsync("sonyslyst@gmail.com", It.IsAny<string>(), It.IsAny<string>(), "kari@kunde.no"), Times.Once);
    }
}
