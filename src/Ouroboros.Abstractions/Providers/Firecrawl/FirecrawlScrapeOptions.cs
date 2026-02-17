namespace Ouroboros.Providers.Firecrawl;

/// <summary>
/// Options for scraping a URL.
/// </summary>
public sealed record FirecrawlScrapeOptions
{
    /// <summary>Gets the output formats requested (markdown, html, rawHtml, links, screenshot).</summary>
    public IReadOnlyList<string> Formats { get; init; } = ["markdown"];

    /// <summary>Gets CSS selectors to include (only content matching these selectors).</summary>
    public IReadOnlyList<string>? IncludeTags { get; init; }

    /// <summary>Gets CSS selectors to exclude.</summary>
    public IReadOnlyList<string>? ExcludeTags { get; init; }

    /// <summary>Gets whether to wait for dynamic content to load.</summary>
    public bool WaitForDynamic { get; init; }

    /// <summary>Gets the timeout in milliseconds for page load.</summary>
    public int? TimeoutMs { get; init; }
}