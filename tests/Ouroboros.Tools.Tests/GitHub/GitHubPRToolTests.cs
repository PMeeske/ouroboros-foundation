// <copyright file="GitHubPRToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.GitHub;

using FluentAssertions;
using Ouroboros.Tools;
using Xunit;

/// <summary>
/// Unit tests for <see cref="GitHubPRTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class GitHubPRToolTests
{
    private const string TestToken = "test-token";
    private const string TestOwner = "test-owner";
    private const string TestRepo = "test-repo";

    private readonly GitHubPRTool tool = new(TestToken, TestOwner, TestRepo);

    #region ITool Interface Tests

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Assert
        this.tool.Name.Should().Be("github_create_pr");
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
    public void Description_ContainsPullRequestKeyword()
    {
        // Assert
        this.tool.Description.Should().Contain("pull request");
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
        this.tool.JsonSchema.Should().Contain("Title");
        this.tool.JsonSchema.Should().Contain("HeadBranch");
        this.tool.JsonSchema.Should().Contain("BaseBranch");
    }

    #endregion

    #region InvokeAsync Validation Tests

    [Fact]
    public async Task InvokeAsync_WithEmptyTitle_ReturnsFailure()
    {
        // Arrange
        string input = """{"Title": "", "HeadBranch": "feature/test"}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("title");
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceTitle_ReturnsFailure()
    {
        // Arrange
        string input = """{"Title": "   ", "HeadBranch": "feature/test"}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("title");
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyHeadBranch_ReturnsFailure()
    {
        // Arrange
        string input = """{"Title": "Test PR", "HeadBranch": ""}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("branch");
    }

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
        string input = """{"Title": "Test PR", "HeadBranch": "feature/test", "BaseBranch": "main"}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Failed to create PR");
    }

    #endregion
}
