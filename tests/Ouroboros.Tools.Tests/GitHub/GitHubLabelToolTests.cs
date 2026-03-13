// <copyright file="GitHubLabelToolTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.GitHub;

using FluentAssertions;
using Ouroboros.Tools;
using Xunit;

/// <summary>
/// Unit tests for <see cref="GitHubLabelTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class GitHubLabelToolTests
{
    private const string TestToken = "test-token";
    private const string TestOwner = "test-owner";
    private const string TestRepo = "test-repo";

    private readonly GitHubLabelTool tool = new(TestToken, TestOwner, TestRepo);

    #region ITool Interface Tests

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Assert
        this.tool.Name.Should().Be("github_manage_labels");
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
    public void Description_ContainsLabelKeyword()
    {
        // Assert
        this.tool.Description.Should().Contain("label");
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
        this.tool.JsonSchema.Should().Contain("AddLabels");
        this.tool.JsonSchema.Should().Contain("RemoveLabels");
    }

    #endregion

    #region InvokeAsync Validation Tests

    [Fact]
    public async Task InvokeAsync_WithNoOperations_ReturnsFailure()
    {
        // Arrange - no AddLabels or RemoveLabels specified
        string input = """{"IssueNumber": 1}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No label operations");
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyArrays_ReturnsFailure()
    {
        // Arrange
        string input = """{"IssueNumber": 1, "AddLabels": [], "RemoveLabels": []}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No label operations");
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
    public async Task InvokeAsync_WithAddLabels_FailsOnNetwork()
    {
        // Arrange - valid input but no real GitHub connection
        string input = """{"IssueNumber": 1, "AddLabels": ["bug"]}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Failed to manage labels");
    }

    #endregion
}
