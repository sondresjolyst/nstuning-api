using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Moq;
using nstuning_api.Features.Auth;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using Xunit;

namespace nstuning_api.Tests;

public class AuthSlicesTests : TestBase
{
    [Fact]
    public async Task Register_NewUser_CreatesUserAndAssignsDefaultRole()
    {
        var user = MakeUser();
        MockMapper.Setup(m => m.Map<User>(It.IsAny<RegisterUserDto>())).Returns(user);
        MockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        MockUserManager.Setup(m => m.CreateAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        MockUserManager.Setup(m => m.AddToRoleAsync(user, "Default")).ReturnsAsync(IdentityResult.Success);

        var dto = new RegisterUserDto { UserName = "u", Email = "a@b.no", FirstName = "A", LastName = "B", Password = "Password1" };
        Assert.IsType<Ok<MessageResponse>>(await Register.Handle(dto, MockUserManager.Object, MockMapper.Object));
        MockUserManager.Verify(m => m.AddToRoleAsync(user, "Default"), Times.Once);
    }

    [Fact]
    public async Task Register_ExistingEmail_ReturnsGenericOkWithoutCreating()
    {
        MockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(MakeUser());

        var dto = new RegisterUserDto { UserName = "u", Email = "a@b.no", FirstName = "A", LastName = "B", Password = "Password1" };
        Assert.IsType<Ok<MessageResponse>>(await Register.Handle(dto, MockUserManager.Object, MockMapper.Object));
        MockUserManager.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        await using var db = CreateDbContext();
        MockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var result = await Login.Handle(new LoginModel { Email = "x@y.no", Password = "nope" },
            MockUserManager.Object, MockSignInManager.Object, Configuration, db);

        var json = Assert.IsType<JsonHttpResult<MessageResponse>>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, json.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        await using var db = CreateDbContext();
        var user = MakeUser();
        MockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        MockSignInManager.Setup(m => m.PasswordSignInAsync(user.UserName!, It.IsAny<string>(), false, true))
            .ReturnsAsync(SignInResult.Success);
        MockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        var result = await Login.Handle(new LoginModel { Email = "a@b.no", Password = "Password1" },
            MockUserManager.Object, MockSignInManager.Object, Configuration, db);

        Assert.IsType<Ok<TokenResponse>>(result);
        Assert.Single(db.RefreshTokens);
    }
}
