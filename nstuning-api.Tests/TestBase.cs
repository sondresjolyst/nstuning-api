using Microsoft.EntityFrameworkCore;
using Mapster;
using MapsterMapper;
using nstuning_api.Profiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using nstuning_api.Constants;
using System.Security.Claims;
using nstuning_api.Models;

namespace nstuning_api.Tests;

/// <summary>The helpers the tests share: an in-memory database and a sample user.</summary>
public abstract class TestBase
{
    protected static IMapper RealMapper { get; } = BuildRealMapper();

    private static IMapper BuildRealMapper()
    {
        var config = new TypeAdapterConfig();
        new MappingProfile().Register(config);
        return new Mapper(config);
    }

    protected static ApplicationDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options);

    protected static ControllerContext MakeControllerContext(string userId = "user-1", bool isAdmin = false)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (isAdmin)
            claims.Add(new(ClaimTypes.Role, RoleNames.Admin));
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
    }

    protected static User MakeUser(string id = "user-1", string email = "test@example.com") => new()
    {
        Id = id,
        Email = email,
        UserName = "testuser",
        FirstName = "Test",
        LastName = "User",
        EmailConfirmed = true
    };
}
