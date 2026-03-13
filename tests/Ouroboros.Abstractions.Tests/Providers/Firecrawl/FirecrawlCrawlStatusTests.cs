namespace Ouroboros.Abstractions.Tests.Providers.Firecrawl;

using Ouroboros.Providers.Firecrawl;

[Trait("Category", "Unit")]
public class FirecrawlCrawlStatusTests
{
    [Fact]
    public void Constructor_RequiredProperties_Set()
    {
        var status = new FirecrawlCrawlStatus
        {
            JobId = "job-123",
            Status = "completed",
        };

        status.JobId.Should().Be("job-123");
        status.Status.Should().Be("completed");
    }

    [Fact]
    public void PagesScraped_DefaultsToZero()
    {
        var status = new FirecrawlCrawlStatus { JobId = "j", Status = "s" };
        status.PagesScraped.Should().Be(0);
    }

    [Fact]
    public void TotalPages_DefaultsToZero()
    {
        var status = new FirecrawlCrawlStatus { JobId = "j", Status = "s" };
        status.TotalPages.Should().Be(0);
    }

    [Fact]
    public void Results_DefaultsToEmpty()
    {
        var status = new FirecrawlCrawlStatus { JobId = "j", Status = "s" };
        status.Results.Should().BeEmpty();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var results = new List<FirecrawlScrapeResult>
        {
            new() { Url = "http://example.com" }
        };

        var status = new FirecrawlCrawlStatus
        {
            JobId = "job-1",
            Status = "scraping",
            PagesScraped = 5,
            TotalPages = 10,
            Results = results,
        };

        status.PagesScraped.Should().Be(5);
        status.TotalPages.Should().Be(10);
        status.Results.Should().HaveCount(1);
    }
}
