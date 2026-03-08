// <copyright file="GitHubToolExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tools;

/// <summary>
/// Extension methods for registering GitHub tools with ToolRegistry.
/// </summary>
public static class GitHubToolExtensions
{
    /// <summary>
    /// Registers all GitHub tools (issues, PRs, search, comments, labels, scope lock).
    /// </summary>
    /// <param name="registry">The tool registry to extend.</param>
    /// <param name="token">GitHub personal access token with repo permissions.</param>
    /// <param name="owner">Repository owner (username or organization).</param>
    /// <param name="repo">Repository name.</param>
    /// <returns>A new ToolRegistry with GitHub tools registered.</returns>
    public static ToolRegistry WithGitHubTools(
        this ToolRegistry registry,
        string token,
        string owner,
        string repo)
    {
        return registry
            .WithTool(new GitHubIssueReadTool(token, owner, repo))
            .WithTool(new GitHubIssueCreateTool(token, owner, repo))
            .WithTool(new GitHubIssueUpdateTool(token, owner, repo))
            .WithTool(new GitHubPRTool(token, owner, repo))
            .WithTool(new GitHubCommentTool(token, owner, repo))
            .WithTool(new GitHubLabelTool(token, owner, repo))
            .WithTool(new GitHubSearchTool(token, owner, repo))
            .WithTool(new GitHubScopeLockTool(token, owner, repo));
    }
}
