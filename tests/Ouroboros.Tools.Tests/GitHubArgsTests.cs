using Ouroboros.Tools;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class GitHubCommentArgsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var args = new GitHubCommentArgs();
        args.IssueNumber.Should().Be(0);
        args.Body.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var args = new GitHubCommentArgs { IssueNumber = 42, Body = "Great work!" };
        args.IssueNumber.Should().Be(42);
        args.Body.Should().Be("Great work!");
    }
}

[Trait("Category", "Unit")]
public class GitHubIssueCreateArgsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var args = new GitHubIssueCreateArgs();
        args.Title.Should().BeEmpty();
        args.Body.Should().BeNull();
        args.Labels.Should().BeNull();
        args.Assignees.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var args = new GitHubIssueCreateArgs
        {
            Title = "Bug report",
            Body = "Description of the bug",
            Labels = new[] { "bug", "urgent" },
            Assignees = new[] { "user1" }
        };
        args.Title.Should().Be("Bug report");
        args.Labels.Should().HaveCount(2);
        args.Assignees.Should().ContainSingle("user1");
    }
}

[Trait("Category", "Unit")]
public class GitHubIssueReadArgsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var args = new GitHubIssueReadArgs();
        args.IssueNumber.Should().Be(0);
    }

    [Fact]
    public void IssueNumber_CanBeSet()
    {
        var args = new GitHubIssueReadArgs { IssueNumber = 123 };
        args.IssueNumber.Should().Be(123);
    }
}

[Trait("Category", "Unit")]
public class GitHubIssueUpdateArgsTests
{
    [Fact]
    public void DefaultValues_AreAllNull()
    {
        var args = new GitHubIssueUpdateArgs();
        args.IssueNumber.Should().Be(0);
        args.State.Should().BeNull();
        args.Title.Should().BeNull();
        args.Body.Should().BeNull();
        args.Labels.Should().BeNull();
        args.Assignees.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var args = new GitHubIssueUpdateArgs
        {
            IssueNumber = 10,
            State = "closed",
            Title = "Updated title",
            Body = "Updated body",
            Labels = new[] { "fixed" },
            Assignees = new[] { "dev1" }
        };
        args.State.Should().Be("closed");
        args.Title.Should().Be("Updated title");
    }
}

[Trait("Category", "Unit")]
public class GitHubLabelArgsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var args = new GitHubLabelArgs();
        args.IssueNumber.Should().Be(0);
        args.AddLabels.Should().BeNull();
        args.RemoveLabels.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var args = new GitHubLabelArgs
        {
            IssueNumber = 5,
            AddLabels = new[] { "enhancement" },
            RemoveLabels = new[] { "wontfix" }
        };
        args.AddLabels.Should().ContainSingle("enhancement");
        args.RemoveLabels.Should().ContainSingle("wontfix");
    }
}

[Trait("Category", "Unit")]
public class GitHubPRArgsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var args = new GitHubPRArgs();
        args.Title.Should().BeEmpty();
        args.HeadBranch.Should().BeEmpty();
        args.BaseBranch.Should().BeNull();
        args.Body.Should().BeNull();
        args.Draft.Should().BeNull();
        args.Labels.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var args = new GitHubPRArgs
        {
            Title = "Add feature X",
            HeadBranch = "feature/x",
            BaseBranch = "develop",
            Body = "Implements feature X",
            Draft = true,
            Labels = new[] { "feature" }
        };
        args.Title.Should().Be("Add feature X");
        args.Draft.Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class GitHubScopeLockArgsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var args = new GitHubScopeLockArgs();
        args.IssueNumber.Should().Be(0);
        args.Milestone.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var args = new GitHubScopeLockArgs { IssueNumber = 7, Milestone = "v1.0" };
        args.IssueNumber.Should().Be(7);
        args.Milestone.Should().Be("v1.0");
    }
}

[Trait("Category", "Unit")]
public class GitHubSearchArgsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var args = new GitHubSearchArgs();
        args.Query.Should().BeEmpty();
        args.Type.Should().BeNull();
        args.MaxResults.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var args = new GitHubSearchArgs { Query = "is:open bug", Type = "issues", MaxResults = 25 };
        args.Query.Should().Be("is:open bug");
        args.Type.Should().Be("issues");
        args.MaxResults.Should().Be(25);
    }
}

[Trait("Category", "Unit")]
public class RetrievalArgsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var args = new RetrievalArgs();
        args.Q.Should().BeEmpty();
        args.K.Should().Be(3);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var args = new RetrievalArgs { Q = "search query", K = 10 };
        args.Q.Should().Be("search query");
        args.K.Should().Be(10);
    }
}
