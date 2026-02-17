namespace Ouroboros.Providers.Firecrawl;

/// <summary>
/// Metadata extracted from a scraped page.
/// </summary>
public sealed record FirecrawlMetadata
{
    /// <summary>Gets the page title.</summary>
    public string? Title { get; init; }

    /// <summary>Gets the meta description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the language.</summary>
    public string? Language { get; init; }

    /// <summary>Gets the source URL.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>Gets the status code.</summary>
    public int? StatusCode { get; init; }
}