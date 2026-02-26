using Ouroboros.Providers.Firecrawl;

namespace Ouroboros.Abstractions.Tests.Providers.Firecrawl;

[Trait("Category", "Unit")]
public class FirecrawlRecordTests
{
    [Fact]
    public void FirecrawlScrapeResult_AllPropertiesSet()
    {
        // Act
        var result = new FirecrawlScrapeResult
        {
            Url = "https://example.com",
            Title = "Example",
            Markdown = "# Example\n\nContent here.",
            Html = "<h1>Example</h1>",
            Metadata = new FirecrawlMetadata
            {
                Title = "Example",
                Description = "An example page",
                Language = "en",
                SourceUrl = "https://example.com",
                StatusCode = 200
            }
        };

        // Assert
        result.Url.Should().Be("https://example.com");
        result.Title.Should().Be("Example");
        result.Markdown.Should().Contain("# Example");
        result.Html.Should().Contain("<h1>");
        result.Metadata.Should().NotBeNull();
        result.Metadata!.StatusCode.Should().Be(200);
    }

    [Fact]
    public void FirecrawlScrapeResult_DefaultOptionals_AreNull()
    {
        // Act
        var result = new FirecrawlScrapeResult { Url = "https://test.com" };

        // Assert
        result.Title.Should().BeNull();
        result.Markdown.Should().BeNull();
        result.Html.Should().BeNull();
        result.Metadata.Should().BeNull();
    }

    [Fact]
    public void FirecrawlMetadata_AllPropertiesSet()
    {
        // Act
        var metadata = new FirecrawlMetadata
        {
            Title = "Page Title",
            Description = "Page description",
            Language = "en",
            SourceUrl = "https://example.com/page",
            StatusCode = 200
        };

        // Assert
        metadata.Title.Should().Be("Page Title");
        metadata.Description.Should().Be("Page description");
        metadata.Language.Should().Be("en");
        metadata.SourceUrl.Should().Be("https://example.com/page");
        metadata.StatusCode.Should().Be(200);
    }

    [Fact]
    public void FirecrawlMetadata_DefaultValues_AreNull()
    {
        // Act
        var metadata = new FirecrawlMetadata();

        // Assert
        metadata.Title.Should().BeNull();
        metadata.Description.Should().BeNull();
        metadata.Language.Should().BeNull();
        metadata.SourceUrl.Should().BeNull();
        metadata.StatusCode.Should().BeNull();
    }

    [Fact]
    public void FirecrawlCrawlOptions_DefaultValues_AreCorrect()
    {
        // Act
        var options = new FirecrawlCrawlOptions();

        // Assert
        options.MaxPages.Should().Be(50);
        options.MaxDepth.Should().Be(3);
        options.IncludePatterns.Should().BeNull();
        options.ExcludePatterns.Should().BeNull();
    }

    [Fact]
    public void FirecrawlCrawlOptions_CustomValues_SetCorrectly()
    {
        // Act
        var options = new FirecrawlCrawlOptions
        {
            MaxPages = 100,
            MaxDepth = 5,
            IncludePatterns = new List<string> { @"\/docs\/" },
            ExcludePatterns = new List<string> { @"\/login" }
        };

        // Assert
        options.MaxPages.Should().Be(100);
        options.MaxDepth.Should().Be(5);
        options.IncludePatterns.Should().HaveCount(1);
        options.ExcludePatterns.Should().HaveCount(1);
    }

    [Fact]
    public void FirecrawlCrawlStatus_AllPropertiesSet()
    {
        // Act
        var status = new FirecrawlCrawlStatus
        {
            JobId = "job-123",
            Status = "completed",
            PagesScraped = 45,
            TotalPages = 50,
            Results = new List<FirecrawlScrapeResult>
            {
                new FirecrawlScrapeResult { Url = "https://example.com/page1" }
            }
        };

        // Assert
        status.JobId.Should().Be("job-123");
        status.Status.Should().Be("completed");
        status.PagesScraped.Should().Be(45);
        status.TotalPages.Should().Be(50);
        status.Results.Should().HaveCount(1);
    }

    [Fact]
    public void FirecrawlCrawlStatus_DefaultResults_AreEmpty()
    {
        // Act
        var status = new FirecrawlCrawlStatus
        {
            JobId = "job-1",
            Status = "scraping"
        };

        // Assert
        status.Results.Should().BeEmpty();
        status.PagesScraped.Should().Be(0);
        status.TotalPages.Should().Be(0);
    }

    [Fact]
    public void FirecrawlScrapeOptions_DefaultValues_AreCorrect()
    {
        // Act
        var options = new FirecrawlScrapeOptions();

        // Assert
        options.Formats.Should().HaveCount(1);
        options.Formats[0].Should().Be("markdown");
        options.IncludeTags.Should().BeNull();
        options.ExcludeTags.Should().BeNull();
        options.WaitForDynamic.Should().BeFalse();
        options.TimeoutMs.Should().BeNull();
    }

    [Fact]
    public void FirecrawlScrapeOptions_CustomValues_SetCorrectly()
    {
        // Act
        var options = new FirecrawlScrapeOptions
        {
            Formats = new List<string> { "markdown", "html" },
            IncludeTags = new List<string> { "article" },
            ExcludeTags = new List<string> { "nav", "footer" },
            WaitForDynamic = true,
            TimeoutMs = 30000
        };

        // Assert
        options.Formats.Should().HaveCount(2);
        options.IncludeTags.Should().Contain("article");
        options.ExcludeTags.Should().HaveCount(2);
        options.WaitForDynamic.Should().BeTrue();
        options.TimeoutMs.Should().Be(30000);
    }

    [Fact]
    public void FirecrawlSearchResult_AllPropertiesSet()
    {
        // Act
        var result = new FirecrawlSearchResult
        {
            Url = "https://example.com/result",
            Title = "Search Result",
            Description = "A relevant page",
            Markdown = "# Content"
        };

        // Assert
        result.Url.Should().Be("https://example.com/result");
        result.Title.Should().Be("Search Result");
        result.Description.Should().Be("A relevant page");
        result.Markdown.Should().Be("# Content");
    }

    [Fact]
    public void FirecrawlSearchResult_DefaultOptionals_AreNull()
    {
        // Act
        var result = new FirecrawlSearchResult { Url = "https://test.com" };

        // Assert
        result.Title.Should().BeNull();
        result.Description.Should().BeNull();
        result.Markdown.Should().BeNull();
    }
}
