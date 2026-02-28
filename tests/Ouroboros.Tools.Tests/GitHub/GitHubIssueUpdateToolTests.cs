// <copyright file="GitHubIssueUpdateToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.GitHub;

using FluentAssertions;
using Ouroboros.Tools;
using Xunit;

/// <summary>
/// Unit tests for <see cref="GitHubIssueUpdateTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class GitHubIssueUpdateToolTests
{
    private const string TestToken = "test-token";
    private const string TestOwner = "test-owner";
    private const string TestRepo = "test-repo";

    private readonly GitHubIssueUpdateTool tool = new(TestToken, TestOwner, TestRepo);

    #region ITool Interface Tests

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Assert
        this.tool.Name.Should().Be("github_update_issue");
    }

    [Fact]
    public void Name_IsNotNullOrEmpty()
    {
        // Assert
        this.tool.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Description_IsNotNullOrEmpty()
    {
        // Assert
        this.tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Description_ContainsUpdateKeyword()
    {
        // Assert
        this.tool.Description.Should().Contain("Update");
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        // Assert
        this.tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void JsonSchema_ContainsExpectedProperties()
    {
        // Assert
        this.tool.JsonSchema.Should().Contain("IssueNumber");
        this.tool.JsonSchema.Should().Contain("State");
        this.tool.JsonSchema.Should().Contain("Title");
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        string input = "not valid json";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithValidInput_FailsOnNetwork()
    {
        // Arrange - valid input but no real GitHub connection
        string input = """{"IssueNumber": 1, "State": "closed"}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Failed to update issue");
    }

    #endregion
}
