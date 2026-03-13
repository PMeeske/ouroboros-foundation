namespace Ouroboros.Abstractions.Tests.Providers.DuckDuckGo;

using Ouroboros.Providers.DuckDuckGo;

[Trait("Category", "Unit")]
public class DuckDuckGoNewsResultTests
{
    [Fact]
    public void Constructor_RequiredProperties_Set()
    {
        var result = new DuckDuckGoNewsResult
        {
            Title = "Breaking News",
            Url = "http://example.com/news",
        };

        result.Title.Should().Be("Breaking News");
        result.Url.Should().Be("http://example.com/news");
    }

    [Fact]
    public void Snippet_DefaultsToNull()
    {
        var result = new DuckDuckGoNewsResult { Title = "t", Url = "u" };
        result.Snippet.Should().BeNull();
    }

    [Fact]
    public void Source_DefaultsToNull()
    {
        var result = new DuckDuckGoNewsResult { Title = "t", Url = "u" };
        result.Source.Should().BeNull();
    }

    [Fact]
    public void PublishedAt_DefaultsToNull()
    {
        var result = new DuckDuckGoNewsResult { Title = "t", Url = "u" };
        result.PublishedAt.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var published = DateTimeOffset.UtcNow;
        var result = new DuckDuckGoNewsResult
        {
            Title = "News",
            Url = "http://url",
            Snippet = "snippet",
            Source = "CNN",
            PublishedAt = published,
        };

        result.Snippet.Should().Be("snippet");
        result.Source.Should().Be("CNN");
        result.PublishedAt.Should().Be(published);
    }
}
