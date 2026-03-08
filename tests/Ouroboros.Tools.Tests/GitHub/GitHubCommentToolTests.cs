// <copyright file="GitHubCommentToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.GitHub;

using FluentAssertions;
using Ouroboros.Tools;
using Xunit;

/// <summary>
/// Unit tests for <see cref="GitHubCommentTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class GitHubCommentToolTests
{
    private const string TestToken = "test-token";
    private const string TestOwner = "test-owner";
    private const string TestRepo = "test-repo";

    private readonly GitHubCommentTool tool = new(TestToken, TestOwner, TestRepo);

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var sut = new GitHubCommentTool(TestToken, TestOwner, TestRepo);

        // Assert
        sut.Should().NotBeNull();
    }

    #endregion

    #region ITool Interface Tests

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Assert
        this.tool.Name.Should().Be("github_add_comment");
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
    public void Description_ContainsCommentKeyword()
    {
        // Assert
        this.tool.Description.Should().Contain("comment");
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
        this.tool.JsonSchema.Should().Contain("Body");
    }

    #endregion

    #region InvokeAsync Validation Tests

    [Fact]
    public async Task InvokeAsync_WithEmptyBody_ReturnsFailure()
    {
        // Arrange
        string input = """{"IssueNumber": 1, "Body": ""}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceBody_ReturnsFailure()
    {
        // Arrange
        string input = """{"IssueNumber": 1, "Body": "   "}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
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
        string input = """{"IssueNumber": 1, "Body": "Test comment"}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert - should fail because we don't have a real connection
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Failed to add comment");
    }

    #endregion
}
