namespace Ouroboros.Abstractions.Tests.Providers.DuckDuckGo;

using Ouroboros.Providers.DuckDuckGo;

[Trait("Category", "Unit")]
public class DuckDuckGoSearchResultTests
{
    [Fact]
    public void Constructor_RequiredProperties_Set()
    {
        var result = new DuckDuckGoSearchResult
        {
            Title = "Search Result",
            Url = "http://example.com",
        };

        result.Title.Should().Be("Search Result");
        result.Url.Should().Be("http://example.com");
    }

    [Fact]
    public void Snippet_DefaultsToNull()
    {
        var result = new DuckDuckGoSearchResult { Title = "t", Url = "u" };
        result.Snippet.Should().BeNull();
    }

    [Fact]
    public void Snippet_CanBeSet()
    {
        var result = new DuckDuckGoSearchResult
        {
            Title = "t",
            Url = "u",
            Snippet = "A description here",
        };

        result.Snippet.Should().Be("A description here");
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var r1 = new DuckDuckGoSearchResult { Title = "t", Url = "u", Snippet = "s" };
        var r2 = new DuckDuckGoSearchResult { Title = "t", Url = "u", Snippet = "s" };
        r1.Should().Be(r2);
    }
}
