namespace Ouroboros.Tools;

/// <summary>
/// Arguments for updating a GitHub issue.
/// </summary>
public sealed class GitHubIssueUpdateArgs
{
    /// <summary>
    /// Gets or sets the issue number.
    /// </summary>
    public int IssueNumber { get; set; }

    /// <summary>
    /// Gets or sets the new state (open or closed).
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Gets or sets the new title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the new body.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the labels (replaces all existing labels).
    /// </summary>
    public string[]? Labels { get; set; }

    /// <summary>
    /// Gets or sets the assignees (replaces all existing assignees).
    /// </summary>
    public string[]? Assignees { get; set; }
}