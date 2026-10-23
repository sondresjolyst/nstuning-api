using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using nstuning_api.Features.Contact;
using nstuning_api.Infrastructure;
using nstuning_api.Models.Admin;
using SjolystInnovation.AppKit.Email;
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
        var email = new Mock<IEmailSender>();

        using var cts = new CancellationTokenSource();

        var req = new ContactRequest("Ola", "ola@kunde.no", "+47 400 00 000", "Volvo <242>", "Hi\nthere & you");
        Assert.IsType<Ok<MessageResponse>>(await SendEnquiry.Handle(req, db, email.Object, cts.Token));

        email.Verify(e => e.SendAsync(
            It.Is<EmailMessage>(m => m.To == "shop@nstuning.no" && m.ReplyTo == "ola@kunde.no" && m.Subject == "New enquiry from Ola"
                && m.HtmlBody.Contains("<p><strong>Name:</strong> Ola</p>")
                && m.HtmlBody.Contains("<p><strong>Email:</strong> ola@kunde.no</p>")
                && m.HtmlBody.Contains("<p><strong>Phone:</strong> +47 400 00 000</p>")
                && m.HtmlBody.Contains("<p><strong>Car:</strong> Volvo &lt;242&gt;</p>")
                && m.HtmlBody.Contains("<p>Hi<br/>there &amp; you</p>")),
            cts.Token), Times.Once);
    }

    [Fact]
    public async Task Send_FallsBackToDefaultRecipientWhenNoSettings()
    {
        await using var db = CreateDbContext();
        var email = new Mock<IEmailSender>();

        await SendEnquiry.Handle(new ContactRequest("Kari", "kari@kunde.no", null, null, "Hei"), db, email.Object, default);

        email.Verify(e => e.SendAsync(It.Is<EmailMessage>(m => m.To == "sonyslyst@gmail.com" && m.ReplyTo == "kari@kunde.no"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("ola@kunde.no", true)]
    [InlineData("  ola@kunde.no  ", true)]
    [InlineData("ola@kunde.no, evil@x.no", false)]
    [InlineData("ola@kunde.no;evil@x.no", false)]
    [InlineData("Ola <ola@kunde.no>", false)]
    [InlineData("ola@kunde.no\r\nBcc: evil@x.no", false)]
    [InlineData("ola@kunde.no\nBcc: evil@x.no", false)]
    [InlineData("ola@kunde.no\rBcc: evil@x.no", false)]
    public void Validator_AcceptsOnlyASingleAddress(string email, bool valid)
    {
        var result = new ContactRequestValidator().Validate(new ContactRequest("Ola", email, null, null, "Hei"));

        Assert.Equal(valid, result.IsValid);
    }

    [Fact]
    public async Task Send_UsesTrimmedAddressAsReplyTo()
    {
        await using var db = CreateDbContext();
        var email = new Mock<IEmailSender>();

        await SendEnquiry.Handle(new ContactRequest("Ola", "  ola@kunde.no ", null, null, "Hei"), db, email.Object, default);

        email.Verify(e => e.SendAsync(It.Is<EmailMessage>(m => m.ReplyTo == "ola@kunde.no"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
