namespace Ouroboros.Tools;

/// <summary>
/// Arguments for managing GitHub labels.
/// </summary>
public sealed class GitHubLabelArgs
{
    /// <summary>
    /// Gets or sets the issue or pull request number.
    /// </summary>
    public int IssueNumber { get; set; }

    /// <summary>
    /// Gets or sets the labels to add.
    /// </summary>
    public string[]? AddLabels { get; set; }

    /// <summary>
    /// Gets or sets the labels to remove.
    /// </summary>
    public string[]? RemoveLabels { get; set; }
}