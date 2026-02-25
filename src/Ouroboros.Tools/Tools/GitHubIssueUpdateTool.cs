using Octokit;

namespace Ouroboros.Tools;

/// <summary>
/// Tool for updating GitHub issues (state, labels, assignees).
/// </summary>
public sealed class GitHubIssueUpdateTool : ITool
{
    private readonly GitHubClient client;
    private readonly string owner;
    private readonly string repo;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubIssueUpdateTool"/> class.
    /// </summary>
    /// <param name="token">GitHub personal access token.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    public GitHubIssueUpdateTool(string token, string owner, string repo)
    {
        this.client = new GitHubClient(new ProductHeaderValue("Ouroboros"))
        {
            Credentials = new Credentials(token),
        };
        this.owner = owner;
        this.repo = repo;
    }

    /// <inheritdoc />
    public string Name => "github_update_issue";

    /// <inheritdoc />
    public string Description => "Update a GitHub issue's state (open/closed), title, body, labels, or assignees.";

    /// <inheritdoc />
    public string JsonSchema => SchemaGenerator.GenerateSchema(typeof(GitHubIssueUpdateArgs));

    /// <inheritdoc />
    public async Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default)
    {
        try
        {
            GitHubIssueUpdateArgs args = ToolJson.Deserialize<GitHubIssueUpdateArgs>(input);

            IssueUpdate update = new IssueUpdate();

            if (args.State != null)
            {
                update.State = args.State.ToLower() == "closed" ? ItemState.Closed : ItemState.Open;
            }

            if (args.Title != null)
            {
                update.Title = args.Title;
            }

            if (args.Body != null)
            {
                update.Body = args.Body;
            }

            if (args.Labels != null)
            {
                update.ClearLabels();
                foreach (string label in args.Labels)
                {
                    update.AddLabel(label);
                }
            }

            if (args.Assignees != null)
            {
                update.ClearAssignees();
                foreach (string assignee in args.Assignees)
                {
                    update.AddAssignee(assignee);
                }
            }

            Issue updatedIssue = await this.client.Issue.Update(this.owner, this.repo, args.IssueNumber, update);

            return Result<string, string>.Success(
                $"✅ Issue #{updatedIssue.Number} updated successfully\n" +
                $"Title: {updatedIssue.Title}\n" +
                $"State: {updatedIssue.State}\n" +
                $"URL: {updatedIssue.HtmlUrl}");
        }
        catch (Exception ex)
        {
            return Result<string, string>.Failure($"Failed to update issue: {ex.Message}");
        }
    }
}