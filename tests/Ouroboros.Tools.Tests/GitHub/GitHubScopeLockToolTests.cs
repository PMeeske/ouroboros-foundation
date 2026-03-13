// <copyright file="GitHubScopeLockToolTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.GitHub;

using FluentAssertions;
using Ouroboros.Tools;
using Xunit;

/// <summary>
/// Unit tests for <see cref="GitHubScopeLockTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class GitHubScopeLockToolTests
{
    private const string TestToken = "test-token";
    private const string TestOwner = "test-owner";
    private const string TestRepo = "test-repo";

    private readonly GitHubScopeLockTool tool = new(TestToken, TestOwner, TestRepo);

    #region ITool Interface Tests

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Assert
        this.tool.Name.Should().Be("github_scope_lock");
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
    public void Description_ContainsScopeKeyword()
    {
        // Assert
        this.tool.Description.Should().Contain("scope");
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
        string input = """{"IssueNumber": 1}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Match(e => e.Contains("Scope lock failed") || e.Contains("Failed to add label"));
    }

    [Fact]
    public async Task InvokeAsync_WithMilestone_FailsOnNetwork()
    {
        // Arrange - valid input with milestone but no real GitHub connection
        string input = """{"IssueNumber": 1, "Milestone": "v1.0"}""";

        // Act
        var result = await this.tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion
}
