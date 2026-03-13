namespace Ouroboros.Abstractions.Tests.Providers.Firecrawl;

using Ouroboros.Providers.Firecrawl;

[Trait("Category", "Unit")]
public class FirecrawlScrapeResultTests
{
    [Fact]
    public void Constructor_RequiredUrl_Set()
    {
        var result = new FirecrawlScrapeResult { Url = "http://example.com" };
        result.Url.Should().Be("http://example.com");
    }

    [Fact]
    public void Title_DefaultsToNull()
    {
        var result = new FirecrawlScrapeResult { Url = "u" };
        result.Title.Should().BeNull();
    }

    [Fact]
    public void Markdown_DefaultsToNull()
    {
        var result = new FirecrawlScrapeResult { Url = "u" };
        result.Markdown.Should().BeNull();
    }

    [Fact]
    public void Html_DefaultsToNull()
    {
        var result = new FirecrawlScrapeResult { Url = "u" };
        result.Html.Should().BeNull();
    }

    [Fact]
    public void Metadata_DefaultsToNull()
    {
        var result = new FirecrawlScrapeResult { Url = "u" };
        result.Metadata.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var metadata = new FirecrawlMetadata { Title = "page" };
        var result = new FirecrawlScrapeResult
        {
            Url = "http://example.com",
            Title = "Page Title",
            Markdown = "# Content",
            Html = "<h1>Content</h1>",
            Metadata = metadata,
        };

        result.Title.Should().Be("Page Title");
        result.Markdown.Should().Be("# Content");
        result.Html.Should().Be("<h1>Content</h1>");
        result.Metadata.Should().BeSameAs(metadata);
    }
}
