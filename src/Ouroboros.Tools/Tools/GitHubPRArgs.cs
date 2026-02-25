namespace Ouroboros.Tools;

/// <summary>
/// Arguments for creating a GitHub pull request.
/// </summary>
public sealed class GitHubPRArgs
{
    /// <summary>
    /// Gets or sets the PR title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the head branch (source branch with changes).
    /// </summary>
    public string HeadBranch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base branch (target branch, defaults to "main").
    /// </summary>
    public string? BaseBranch { get; set; }

    /// <summary>
    /// Gets or sets the PR body/description.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the PR should be created as a draft.
    /// </summary>
    public bool? Draft { get; set; }

    /// <summary>
    /// Gets or sets the labels to apply to the PR.
    /// </summary>
    public string[]? Labels { get; set; }
}