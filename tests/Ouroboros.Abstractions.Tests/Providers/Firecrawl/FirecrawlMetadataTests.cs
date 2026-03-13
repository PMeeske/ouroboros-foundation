namespace Ouroboros.Abstractions.Tests.Providers.Firecrawl;

using Ouroboros.Providers.Firecrawl;

[Trait("Category", "Unit")]
public class FirecrawlMetadataTests
{
    [Fact]
    public void DefaultInstance_AllPropertiesNull()
    {
        var meta = new FirecrawlMetadata();
        meta.Title.Should().BeNull();
        meta.Description.Should().BeNull();
        meta.Language.Should().BeNull();
        meta.SourceUrl.Should().BeNull();
        meta.StatusCode.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var meta = new FirecrawlMetadata
        {
            Title = "Page Title",
            Description = "A description",
            Language = "en",
            SourceUrl = "http://example.com",
            StatusCode = 200,
        };

        meta.Title.Should().Be("Page Title");
        meta.Description.Should().Be("A description");
        meta.Language.Should().Be("en");
        meta.SourceUrl.Should().Be("http://example.com");
        meta.StatusCode.Should().Be(200);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var m1 = new FirecrawlMetadata { Title = "t", Language = "en" };
        var m2 = new FirecrawlMetadata { Title = "t", Language = "en" };
        m1.Should().Be(m2);
    }
}
