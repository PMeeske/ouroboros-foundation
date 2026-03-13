using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Tools;

/// <summary>
/// Arguments for reading a GitHub issue.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class GitHubIssueReadArgs
{
    /// <summary>
    /// Gets or sets the issue number.
    /// </summary>
    public int IssueNumber { get; set; }
}