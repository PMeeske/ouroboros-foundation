namespace Ouroboros.Tools;

/// <summary>
/// Arguments for the GitHubScopeLockTool.
/// </summary>
public sealed class GitHubScopeLockArgs
{
    /// <summary>
    /// Gets or sets the GitHub issue number to lock.
    /// </summary>
    public int IssueNumber { get; set; }

    /// <summary>
    /// Gets or sets the optional milestone name to assign to the issue.
    /// </summary>
    public string? Milestone { get; set; }
}