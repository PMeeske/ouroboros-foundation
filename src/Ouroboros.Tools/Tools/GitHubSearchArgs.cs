namespace Ouroboros.Tools;

/// <summary>
/// Arguments for GitHub search.
/// </summary>
public sealed class GitHubSearchArgs
{
    /// <summary>
    /// Gets or sets the search query.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the search type (issues, code). Defaults to issues.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of results to return (default 10, max 100).
    /// </summary>
    public int? MaxResults { get; set; }
}