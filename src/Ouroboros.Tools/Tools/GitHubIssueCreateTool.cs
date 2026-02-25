// <copyright file="GitHubIssueTools.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tools;

using Octokit;

/// <summary>
/// Tool for creating new GitHub issues.
/// </summary>
public sealed class GitHubIssueCreateTool : ITool
{
    private readonly GitHubClient client;
    private readonly string owner;
    private readonly string repo;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubIssueCreateTool"/> class.
    /// </summary>
    /// <param name="token">GitHub personal access token with repo permissions.</param>
    /// <param name="owner">Repository owner (username or organization).</param>
    /// <param name="repo">Repository name.</param>
    public GitHubIssueCreateTool(string token, string owner, string repo)
    {
        this.client = new GitHubClient(new ProductHeaderValue("Ouroboros"))
        {
            Credentials = new Credentials(token),
        };
        this.owner = owner;
        this.repo = repo;
    }

    /// <inheritdoc />
    public string Name => "github_create_issue";

    /// <inheritdoc />
    public string Description => "Create a new GitHub issue with title, body, labels, and optional assignees. Returns the created issue URL.";

    /// <inheritdoc />
    public string JsonSchema => SchemaGenerator.GenerateSchema(typeof(GitHubIssueCreateArgs));

    /// <inheritdoc />
    public async Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        try
        {
            GitHubIssueCreateArgs args = ToolJson.Deserialize<GitHubIssueCreateArgs>(input);

            if (string.IsNullOrWhiteSpace(args.Title))
            {
                return Result<string, string>.Failure("Issue title cannot be empty");
            }

            NewIssue newIssue = new NewIssue(args.Title)
            {
                Body = args.Body ?? string.Empty,
            };

            // Add labels if provided
            if (args.Labels != null)
            {
                foreach (string label in args.Labels)
                {
                    newIssue.Labels.Add(label);
                }
            }

            // Add assignees if provided
            if (args.Assignees != null)
            {
                foreach (string assignee in args.Assignees)
                {
                    newIssue.Assignees.Add(assignee);
                }
            }

            Issue issue = await this.client.Issue.Create(this.owner, this.repo, newIssue);

            return Result<string, string>.Success(
                $"✅ Issue #{issue.Number} created successfully\n" +
                $"Title: {issue.Title}\n" +
                $"URL: {issue.HtmlUrl}");
        }
        catch (Exception ex)
        {
            return Result<string, string>.Failure($"Failed to create issue: {ex.Message}");
        }
    }
}