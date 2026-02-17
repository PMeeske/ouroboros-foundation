using Octokit;

namespace Ouroboros.Tools;

/// <summary>
/// Tool for reading GitHub issue details.
/// </summary>
public sealed class GitHubIssueReadTool : ITool
{
    private readonly GitHubClient client;
    private readonly string owner;
    private readonly string repo;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubIssueReadTool"/> class.
    /// </summary>
    /// <param name="token">GitHub personal access token.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    public GitHubIssueReadTool(string token, string owner, string repo)
    {
        this.client = new GitHubClient(new ProductHeaderValue("Ouroboros"))
        {
            Credentials = new Credentials(token),
        };
        this.owner = owner;
        this.repo = repo;
    }

    /// <inheritdoc />
    public string Name => "github_read_issue";

    /// <inheritdoc />
    public string Description => "Read detailed information about a specific GitHub issue including title, body, state, labels, and comments.";

    /// <inheritdoc />
    public string JsonSchema => SchemaGenerator.GenerateSchema(typeof(GitHubIssueReadArgs));

    /// <inheritdoc />
    public async Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        try
        {
            GitHubIssueReadArgs args = ToolJson.Deserialize<GitHubIssueReadArgs>(input);

            Issue issue = await this.client.Issue.Get(this.owner, this.repo, args.IssueNumber);

            string result = $"Issue #{issue.Number}: {issue.Title}\n" +
                            $"State: {issue.State}\n" +
                            $"Author: {issue.User.Login}\n" +
                            $"Created: {issue.CreatedAt:yyyy-MM-dd HH:mm}\n" +
                            $"Updated: {issue.UpdatedAt:yyyy-MM-dd HH:mm}\n" +
                            $"Labels: {string.Join(", ", issue.Labels.Select(l => l.Name))}\n" +
                            $"Assignees: {string.Join(", ", issue.Assignees.Select(a => a.Login))}\n" +
                            $"Comments: {issue.Comments}\n" +
                            $"\nBody:\n{issue.Body ?? "(no body)"}\n" +
                            $"\nURL: {issue.HtmlUrl}";

            return Result<string, string>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<string, string>.Failure($"Failed to read issue: {ex.Message}");
        }
    }
}