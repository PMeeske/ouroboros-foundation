namespace Ouroboros.Tools;

/// <summary>
/// Arguments for creating a GitHub issue.
/// </summary>
public sealed class GitHubIssueCreateArgs
{
    /// <summary>
    /// Gets or sets the issue title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issue body/description.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the labels to apply to the issue.
    /// </summary>
    public string[]? Labels { get; set; }

    /// <summary>
    /// Gets or sets the usernames to assign to the issue.
    /// </summary>
    public string[]? Assignees { get; set; }
}