// <copyright file="GitHubSearchToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.GitHub;

using FluentAssertions;
using Ouroboros.Tools;
using Xunit;

/// <summary>
/// Unit tests for <see cref="GitHubSearchTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class GitHubSearchToolTests
{
    private const string TestToken = "test-token";
    private const string TestOwner = "test-owner";
    private const string TestRepo = "test-repo";

    private readonly GitHubSearchTool tool = new(TestToken, TestOwner, TestRepo);

    #region ITool Interface Tests

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Assert
        this.tool.Name.Should().Be("github_search");
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
    public void Description_ContainsSearchKeyword()
    {
        // Assert
        this.tool.Description.Should().Contain("Search");
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
        this.tool.JsonSchema.Should().Contain("Query");
        this.tool.JsonSchema.Should().Contain("Type");
        this.tool.JsonSchema.Should().Contain("MaxResults");
    }

    #endregion

    #region InvokeAsync Validation Tests

    [Fact]
    public async Task InvokeAsync_WithEmptyQuery_ReturnsFailure()
    {
        // Arrange
        string input = """{"Query": ""}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceQuery_ReturnsFailure()
    {
        // Arrange
        string input = """{"Query": "   "}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidSearchType_ReturnsFailure()
    {
        // Arrange
        string input = """{"Query": "test", "Type": "invalid_type"}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Unknown search type");
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
    public async Task InvokeAsync_WithValidIssueSearch_FailsOnNetwork()
    {
        // Arrange - valid input but no real GitHub connection
        string input = """{"Query": "bug", "Type": "issues"}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("failed");
    }

    [Fact]
    public async Task InvokeAsync_WithValidCodeSearch_FailsOnNetwork()
    {
        // Arrange - valid input but no real GitHub connection
        string input = """{"Query": "function", "Type": "code"}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("failed");
    }

    #endregion
}
