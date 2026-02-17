namespace Ouroboros.Providers.DuckDuckGo;

/// <summary>
/// A DuckDuckGo news search result.
/// </summary>
public sealed record DuckDuckGoNewsResult
{
    /// <summary>Gets the news title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the article URL.</summary>
    public required string Url { get; init; }

    /// <summary>Gets the news snippet.</summary>
    public string? Snippet { get; init; }

    /// <summary>Gets the source/publisher name.</summary>
    public string? Source { get; init; }

    /// <summary>Gets the publication date.</summary>
    public DateTimeOffset? PublishedAt { get; init; }
}