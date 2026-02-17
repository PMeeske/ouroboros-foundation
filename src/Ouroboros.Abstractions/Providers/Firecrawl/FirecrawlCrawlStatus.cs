namespace Ouroboros.Providers.Firecrawl;

/// <summary>
/// Status of a crawl job.
/// </summary>
public sealed record FirecrawlCrawlStatus
{
    /// <summary>Gets the job ID.</summary>
    public required string JobId { get; init; }

    /// <summary>Gets the status (scraping, completed, failed).</summary>
    public required string Status { get; init; }

    /// <summary>Gets the number of pages scraped so far.</summary>
    public int PagesScraped { get; init; }

    /// <summary>Gets the total pages discovered.</summary>
    public int TotalPages { get; init; }

    /// <summary>Gets the scraped results (available when completed).</summary>
    public IReadOnlyList<FirecrawlScrapeResult> Results { get; init; } = [];
}