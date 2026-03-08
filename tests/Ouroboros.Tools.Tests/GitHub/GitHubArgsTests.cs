// <copyright file="GitHubArgsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.GitHub;

using FluentAssertions;
using Ouroboros.Tools;
using Xunit;

/// <summary>
/// Unit tests for GitHub Args record/class types.
/// </summary>
[Trait("Category", "Unit")]
public class GitHubArgsTests
{
    #region GitHubCommentArgs Tests

    [Fact]
    public void GitHubCommentArgs_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var args = new GitHubCommentArgs();

        // Assert
        args.IssueNumber.Should().Be(0);
        args.Body.Should().Be(string.Empty);
    }

    [Fact]
    public void GitHubCommentArgs_SetProperties_RetainsValues()
    {
        // Arrange & Act
        var args = new GitHubCommentArgs
        {
            IssueNumber = 42,
            Body = "Test comment body",
        };

        // Assert
        args.IssueNumber.Should().Be(42);
        args.Body.Should().Be("Test comment body");
    }

    [Fact]
    public void GitHubCommentArgs_CanDeserializeFromJson()
    {
        // Arrange
        string json = """{"IssueNumber": 5, "Body": "Hello world"}""";

        // Act
        var args = ToolJson.Deserialize<GitHubCommentArgs>(json);

        // Assert
        args.IssueNumber.Should().Be(5);
        args.Body.Should().Be("Hello world");
    }

    #endregion

    #region GitHubIssueCreateArgs Tests

    [Fact]
    public void GitHubIssueCreateArgs_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var args = new GitHubIssueCreateArgs();

        // Assert
        args.Title.Should().Be(string.Empty);
        args.Body.Should().BeNull();
        args.Labels.Should().BeNull();
        args.Assignees.Should().BeNull();
    }

    [Fact]
    public void GitHubIssueCreateArgs_SetProperties_RetainsValues()
    {
        // Arrange & Act
        var args = new GitHubIssueCreateArgs
        {
            Title = "Test Issue",
            Body = "Issue body",
            Labels = new[] { "bug", "urgent" },
            Assignees = new[] { "user1" },
        };

        // Assert
        args.Title.Should().Be("Test Issue");
        args.Body.Should().Be("Issue body");
        args.Labels.Should().HaveCount(2);
        args.Assignees.Should().HaveCount(1);
    }

    [Fact]
    public void GitHubIssueCreateArgs_CanDeserializeFromJson()
    {
        // Arrange
        string json = """{"Title": "Bug Report", "Body": "Details", "Labels": ["bug"], "Assignees": ["dev1"]}""";

        // Act
        var args = ToolJson.Deserialize<GitHubIssueCreateArgs>(json);

        // Assert
        args.Title.Should().Be("Bug Report");
        args.Body.Should().Be("Details");
        args.Labels.Should().Contain("bug");
        args.Assignees.Should().Contain("dev1");
    }

    [Fact]
    public void GitHubIssueCreateArgs_CanDeserializeWithNullOptionalFields()
    {
        // Arrange
        string json = """{"Title": "Simple Issue"}""";

        // Act
        var args = ToolJson.Deserialize<GitHubIssueCreateArgs>(json);

        // Assert
        args.Title.Should().Be("Simple Issue");
        args.Body.Should().BeNull();
        args.Labels.Should().BeNull();
        args.Assignees.Should().BeNull();
    }

    #endregion

    #region GitHubIssueReadArgs Tests

    [Fact]
    public void GitHubIssueReadArgs_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var args = new GitHubIssueReadArgs();

        // Assert
        args.IssueNumber.Should().Be(0);
    }

    [Fact]
    public void GitHubIssueReadArgs_CanDeserializeFromJson()
    {
        // Arrange
        string json = """{"IssueNumber": 42}""";

        // Act
        var args = ToolJson.Deserialize<GitHubIssueReadArgs>(json);

        // Assert
        args.IssueNumber.Should().Be(42);
    }

    #endregion

    #region GitHubIssueUpdateArgs Tests

    [Fact]
    public void GitHubIssueUpdateArgs_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var args = new GitHubIssueUpdateArgs();

        // Assert
        args.IssueNumber.Should().Be(0);
        args.State.Should().BeNull();
        args.Title.Should().BeNull();
        args.Body.Should().BeNull();
        args.Labels.Should().BeNull();
        args.Assignees.Should().BeNull();
    }

    [Fact]
    public void GitHubIssueUpdateArgs_CanDeserializeFromJson()
    {
        // Arrange
        string json = """{"IssueNumber": 10, "State": "closed", "Title": "Updated Title"}""";

        // Act
        var args = ToolJson.Deserialize<GitHubIssueUpdateArgs>(json);

        // Assert
        args.IssueNumber.Should().Be(10);
        args.State.Should().Be("closed");
        args.Title.Should().Be("Updated Title");
    }

    [Fact]
    public void GitHubIssueUpdateArgs_CanDeserializeWithLabelsAndAssignees()
    {
        // Arrange
        string json = """{"IssueNumber": 10, "Labels": ["bug", "wontfix"], "Assignees": ["user1", "user2"]}""";

        // Act
        var args = ToolJson.Deserialize<GitHubIssueUpdateArgs>(json);

        // Assert
        args.Labels.Should().HaveCount(2);
        args.Labels.Should().Contain("bug");
        args.Labels.Should().Contain("wontfix");
        args.Assignees.Should().HaveCount(2);
    }

    #endregion

    #region GitHubLabelArgs Tests

    [Fact]
    public void GitHubLabelArgs_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var args = new GitHubLabelArgs();

        // Assert
        args.IssueNumber.Should().Be(0);
        args.AddLabels.Should().BeNull();
        args.RemoveLabels.Should().BeNull();
    }

    [Fact]
    public void GitHubLabelArgs_CanDeserializeFromJson()
    {
        // Arrange
        string json = """{"IssueNumber": 5, "AddLabels": ["enhancement"], "RemoveLabels": ["bug"]}""";

        // Act
        var args = ToolJson.Deserialize<GitHubLabelArgs>(json);

        // Assert
        args.IssueNumber.Should().Be(5);
        args.AddLabels.Should().Contain("enhancement");
        args.RemoveLabels.Should().Contain("bug");
    }

    #endregion

    #region GitHubPRArgs Tests

    [Fact]
    public void GitHubPRArgs_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var args = new GitHubPRArgs();

        // Assert
        args.Title.Should().Be(string.Empty);
        args.HeadBranch.Should().Be(string.Empty);
        args.BaseBranch.Should().BeNull();
        args.Body.Should().BeNull();
        args.Draft.Should().BeNull();
        args.Labels.Should().BeNull();
    }

    [Fact]
    public void GitHubPRArgs_CanDeserializeFromJson()
    {
        // Arrange
        string json = """{"Title": "Add feature", "HeadBranch": "feature/x", "BaseBranch": "develop", "Draft": true, "Labels": ["review"]}""";

        // Act
        var args = ToolJson.Deserialize<GitHubPRArgs>(json);

        // Assert
        args.Title.Should().Be("Add feature");
        args.HeadBranch.Should().Be("feature/x");
        args.BaseBranch.Should().Be("develop");
        args.Draft.Should().BeTrue();
        args.Labels.Should().Contain("review");
    }

    [Fact]
    public void GitHubPRArgs_WithMinimalFields_CanDeserialize()
    {
        // Arrange
        string json = """{"Title": "Fix", "HeadBranch": "fix/bug"}""";

        // Act
        var args = ToolJson.Deserialize<GitHubPRArgs>(json);

        // Assert
        args.Title.Should().Be("Fix");
        args.HeadBranch.Should().Be("fix/bug");
        args.BaseBranch.Should().BeNull();
        args.Draft.Should().BeNull();
    }

    #endregion

    #region GitHubSearchArgs Tests

    [Fact]
    public void GitHubSearchArgs_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var args = new GitHubSearchArgs();

        // Assert
        args.Query.Should().Be(string.Empty);
        args.Type.Should().BeNull();
        args.MaxResults.Should().BeNull();
    }

    [Fact]
    public void GitHubSearchArgs_CanDeserializeFromJson()
    {
        // Arrange
        string json = """{"Query": "authentication", "Type": "issues", "MaxResults": 5}""";

        // Act
        var args = ToolJson.Deserialize<GitHubSearchArgs>(json);

        // Assert
        args.Query.Should().Be("authentication");
        args.Type.Should().Be("issues");
        args.MaxResults.Should().Be(5);
    }

    #endregion

    #region GitHubScopeLockArgs Tests

    [Fact]
    public void GitHubScopeLockArgs_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var args = new GitHubScopeLockArgs();

        // Assert
        args.IssueNumber.Should().Be(0);
        args.Milestone.Should().BeNull();
    }

    [Fact]
    public void GitHubScopeLockArgs_CanDeserializeFromJson()
    {
        // Arrange
        string json = """{"IssueNumber": 7, "Milestone": "v2.0"}""";

        // Act
        var args = ToolJson.Deserialize<GitHubScopeLockArgs>(json);

        // Assert
        args.IssueNumber.Should().Be(7);
        args.Milestone.Should().Be("v2.0");
    }

    [Fact]
    public void GitHubScopeLockArgs_WithoutMilestone_CanDeserialize()
    {
        // Arrange
        string json = """{"IssueNumber": 7}""";

        // Act
        var args = ToolJson.Deserialize<GitHubScopeLockArgs>(json);

        // Assert
        args.IssueNumber.Should().Be(7);
        args.Milestone.Should().BeNull();
    }

    #endregion
}
