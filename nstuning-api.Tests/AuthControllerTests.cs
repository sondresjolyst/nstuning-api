using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using nstuning_api.Dtos.Auth;
using nstuning_api.Models;
using Xunit;

namespace nstuning_api.Tests;

public class AuthControllerTests : TestBase
{
    [Fact]
    public async Task Register_NewUser_CreatesUserAndAssignsDefaultRole()
    {
        await using var db = CreateDbContext();
        var user = MakeUser();
        MockMapper.Setup(m => m.Map<User>(It.IsAny<RegisterUserDto>())).Returns(user);
        MockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        MockUserManager.Setup(m => m.CreateAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        MockUserManager.Setup(m => m.AddToRoleAsync(user, "Default")).ReturnsAsync(IdentityResult.Success);

        var controller = CreateAuthController(db);
        var dto = new RegisterUserDto { UserName = "u", Email = "a@b.no", FirstName = "A", LastName = "B", Password = "Password1" };

        Assert.IsType<OkObjectResult>(await controller.Register(dto));
        MockUserManager.Verify(m => m.AddToRoleAsync(user, "Default"), Times.Once);
    }

    [Fact]
    public async Task Register_ExistingEmail_ReturnsGenericOkWithoutCreating()
    {
        await using var db = CreateDbContext();
        MockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(MakeUser());

        var controller = CreateAuthController(db);
        var dto = new RegisterUserDto { UserName = "u", Email = "a@b.no", FirstName = "A", LastName = "B", Password = "Password1" };

        Assert.IsType<OkObjectResult>(await controller.Register(dto));
        MockUserManager.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        await using var db = CreateDbContext();
        MockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var controller = CreateAuthController(db);
        var result = await controller.Login(new LoginModel { Email = "x@y.no", Password = "nope" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        await using var db = CreateDbContext();
        var user = MakeUser();
        MockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        MockSignInManager
            .Setup(m => m.PasswordSignInAsync(user.UserName!, It.IsAny<string>(), false, true))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        MockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        var controller = CreateAuthController(db);
        var result = Assert.IsType<OkObjectResult>(await controller.Login(new LoginModel { Email = "a@b.no", Password = "Password1" }));

        Assert.NotNull(result.Value);
        Assert.Single(db.RefreshTokens);
    }
}
