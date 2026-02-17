namespace Ouroboros.Providers.Firecrawl;

/// <summary>
/// A search result with scraped content.
/// </summary>
public sealed record FirecrawlSearchResult
{
    /// <summary>Gets the result URL.</summary>
    public required string Url { get; init; }

    /// <summary>Gets the page title.</summary>
    public string? Title { get; init; }

    /// <summary>Gets a snippet or description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the markdown content of the result page.</summary>
    public string? Markdown { get; init; }
}