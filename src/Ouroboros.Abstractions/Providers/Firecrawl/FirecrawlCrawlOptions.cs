namespace Ouroboros.Providers.Firecrawl;

/// <summary>
/// Options for crawling a website.
/// </summary>
public sealed record FirecrawlCrawlOptions
{
    /// <summary>Gets the maximum number of pages to crawl (default: 50).</summary>
    public int MaxPages { get; init; } = 50;

    /// <summary>Gets URL patterns to include (regex).</summary>
    public IReadOnlyList<string>? IncludePatterns { get; init; }

    /// <summary>Gets URL patterns to exclude (regex).</summary>
    public IReadOnlyList<string>? ExcludePatterns { get; init; }

    /// <summary>Gets the maximum crawl depth (default: 3).</summary>
    public int MaxDepth { get; init; } = 3;
}