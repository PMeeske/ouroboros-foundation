namespace Ouroboros.Tools;

/// <summary>
/// Arguments for adding a comment to a GitHub issue/PR.
/// </summary>
public sealed class GitHubCommentArgs
{
    /// <summary>
    /// Gets or sets the issue or pull request number.
    /// </summary>
    public int IssueNumber { get; set; }

    /// <summary>
    /// Gets or sets the comment body (supports markdown).
    /// </summary>
    public string Body { get; set; } = string.Empty;
}