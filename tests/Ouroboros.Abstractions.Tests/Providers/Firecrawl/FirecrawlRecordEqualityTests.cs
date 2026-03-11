using Ouroboros.Providers.Firecrawl;

namespace Ouroboros.Abstractions.Tests.Providers.Firecrawl;

/// <summary>
/// Additional edge case and equality tests for Firecrawl record types.
/// </summary>
[Trait("Category", "Unit")]
public class FirecrawlRecordEqualityTests
{
    [Fact]
    public void FirecrawlScrapeResult_WithMetadata_MetadataAccessible()
    {
        // Act
        var result = new FirecrawlScrapeResult
        {
            Url = "https://example.com",
            Metadata = new FirecrawlMetadata
            {
                Title = "Example",
                Description = "Desc",
                Language = "en",
                SourceUrl = "https://example.com",
                StatusCode = 200
            }
        };

        // Assert
        result.Metadata.Should().NotBeNull();
        result.Metadata!.Title.Should().Be("Example");
        result.Metadata.StatusCode.Should().Be(200);
    }

    [Fact]
    public void FirecrawlCrawlOptions_WithExpression_Patterns()
    {
        // Arrange
        var options = new FirecrawlCrawlOptions
        {
            MaxPages = 100,
            MaxDepth = 5,
            IncludePatterns = new List<string> { @"\/docs\/" },
            ExcludePatterns = new List<string> { @"\/login", @"\/admin" }
        };

        // Assert
        options.IncludePatterns.Should().HaveCount(1);
        options.ExcludePatterns.Should().HaveCount(2);
    }

    [Fact]
    public void FirecrawlCrawlStatus_InProgress_HasPartialResults()
    {
        // Act
        var status = new FirecrawlCrawlStatus
        {
            JobId = "job-456",
            Status = "scraping",
            PagesScraped = 20,
            TotalPages = 100,
            Results = new List<FirecrawlScrapeResult>
            {
                new() { Url = "https://example.com/1" },
                new() { Url = "https://example.com/2" }
            }
        };

        // Assert
        status.Status.Should().Be("scraping");
        status.PagesScraped.Should().BeLessThan(status.TotalPages);
        status.Results.Should().HaveCount(2);
    }

    [Fact]
    public void FirecrawlScrapeOptions_WithAllFormats_FormatsCorrect()
    {
        // Act
        var options = new FirecrawlScrapeOptions
        {
            Formats = new List<string> { "markdown", "html", "text" },
            WaitForDynamic = true,
            TimeoutMs = 60000
        };

        // Assert
        options.Formats.Should().HaveCount(3);
        options.Formats.Should().Contain("text");
        options.WaitForDynamic.Should().BeTrue();
        options.TimeoutMs.Should().Be(60000);
    }

    [Fact]
    public void FirecrawlSearchResult_WithAllFields_AllAccessible()
    {
        // Act
        var result = new FirecrawlSearchResult
        {
            Url = "https://example.com/result",
            Title = "Search Result Title",
            Description = "A search result description",
            Markdown = "# Result\n\nContent"
        };

        // Assert
        result.Url.Should().Be("https://example.com/result");
        result.Title.Should().Be("Search Result Title");
        result.Description.Should().Contain("search result");
        result.Markdown.Should().Contain("# Result");
    }

    [Fact]
    public void FirecrawlMetadata_StatusCode_ErrorCodes()
    {
        // Act
        var metadata = new FirecrawlMetadata
        {
            SourceUrl = "https://missing.com",
            StatusCode = 404
        };

        // Assert
        metadata.StatusCode.Should().Be(404);
    }
}
