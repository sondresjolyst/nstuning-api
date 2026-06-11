using nstuning_api.Helpers;
using Xunit;

namespace nstuning_api.Tests;

public class SlugifyTests
{
    [Theory]
    [InlineData("Golf R Stage 2", "golf-r-stage-2")]
    [InlineData("  Spaced   Out  ", "spaced-out")]
    [InlineData("Audi RS6 / 600hp!", "audi-rs6-600hp")]
    [InlineData("Blåbær Æøå", "blabaer-aeoa")]
    public void Create_ProducesExpectedSlug(string input, string expected)
    {
        Assert.Equal(expected, Slugify.Create(input));
    }

    [Fact]
    public void Create_EmptyInput_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, Slugify.Create("   "));
    }
}
