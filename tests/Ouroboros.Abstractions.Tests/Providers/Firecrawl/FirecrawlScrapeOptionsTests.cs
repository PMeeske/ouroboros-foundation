namespace Ouroboros.Abstractions.Tests.Providers.Firecrawl;

using Ouroboros.Providers.Firecrawl;

[Trait("Category", "Unit")]
public class FirecrawlScrapeOptionsTests
{
    [Fact]
    public void Formats_DefaultsToMarkdown()
    {
        var options = new FirecrawlScrapeOptions();
        options.Formats.Should().ContainSingle().Which.Should().Be("markdown");
    }

    [Fact]
    public void IncludeTags_DefaultsToNull()
    {
        var options = new FirecrawlScrapeOptions();
        options.IncludeTags.Should().BeNull();
    }

    [Fact]
    public void ExcludeTags_DefaultsToNull()
    {
        var options = new FirecrawlScrapeOptions();
        options.ExcludeTags.Should().BeNull();
    }

    [Fact]
    public void WaitForDynamic_DefaultsToFalse()
    {
        var options = new FirecrawlScrapeOptions();
        options.WaitForDynamic.Should().BeFalse();
    }

    [Fact]
    public void TimeoutMs_DefaultsToNull()
    {
        var options = new FirecrawlScrapeOptions();
        options.TimeoutMs.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var options = new FirecrawlScrapeOptions
        {
            Formats = new[] { "markdown", "html" },
            IncludeTags = new[] { "article" },
            ExcludeTags = new[] { "nav", "footer" },
            WaitForDynamic = true,
            TimeoutMs = 5000,
        };

        options.Formats.Should().HaveCount(2);
        options.IncludeTags.Should().ContainSingle();
        options.ExcludeTags.Should().HaveCount(2);
        options.WaitForDynamic.Should().BeTrue();
        options.TimeoutMs.Should().Be(5000);
    }
}
