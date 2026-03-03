// <copyright file="ModelClassTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using Microsoft.CodeAnalysis;
using Ouroboros.Tools;

/// <summary>
/// Tests for model/DTO classes: ToolInfo, ValidationResult, ToolExecutionResult,
/// DslSuggestion, CodeAnalysisResult, MockMcpServer, GitHubToolExtensions.
/// </summary>
[Trait("Category", "Unit")]
public class ModelClassTests
{
    #region ToolInfo Tests

    [Fact]
    public void ToolInfo_ConstructorSetsProperties()
    {
        // Act
        var info = new ToolInfo("my-tool", "My description", new { type = "object" });

        // Assert
        info.Name.Should().Be("my-tool");
        info.Description.Should().Be("My description");
        info.InputSchema.Should().NotBeNull();
    }

    [Fact]
    public void ToolInfo_InputSchemaCanBeComplex()
    {
        // Act
        var schema = new { type = "object", properties = new { name = "string" } };
        var info = new ToolInfo("t", "d", schema);

        // Assert
        info.InputSchema.Should().Be(schema);
    }

    #endregion

    #region ValidationResult Tests

    [Fact]
    public void ValidationResult_ValidResult_HasNoErrors()
    {
        // Act
        var result = new ValidationResult(true, Array.Empty<string>(), Array.Empty<string>());

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Suggestions.Should().BeEmpty();
    }

    [Fact]
    public void ValidationResult_InvalidResult_HasErrors()
    {
        // Act
        var result = new ValidationResult(false,
            new[] { "Error 1", "Error 2" },
            new[] { "Suggestion 1" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Suggestions.Should().HaveCount(1);
    }

    #endregion

    #region ToolExecutionResult Tests

    [Fact]
    public void ToolExecutionResult_SuccessfulExecution_Properties()
    {
        // Act
        var result = new ToolExecutionResult(true, "output data");

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().Be("output data");
    }

    [Fact]
    public void ToolExecutionResult_FailedExecution_Properties()
    {
        // Act
        var result = new ToolExecutionResult(false, "error message");

        // Assert
        result.Success.Should().BeFalse();
        result.Result.Should().Be("error message");
    }

    #endregion

    #region DslSuggestion Tests

    [Fact]
    public void DslSuggestion_ConstructorSetsAllProperties()
    {
        // Act
        var suggestion = new DslSuggestion("UseDraft", "Generate initial draft", 0.9);

        // Assert
        suggestion.Step.Should().Be("UseDraft");
        suggestion.Explanation.Should().Be("Generate initial draft");
        suggestion.Confidence.Should().Be(0.9);
    }

    [Fact]
    public void DslSuggestion_ZeroConfidence_IsValid()
    {
        // Act
        var suggestion = new DslSuggestion("LowConfidence", "Not sure", 0.0);

        // Assert
        suggestion.Confidence.Should().Be(0.0);
    }

    [Fact]
    public void DslSuggestion_MaxConfidence_IsValid()
    {
        // Act
        var suggestion = new DslSuggestion("HighConfidence", "Very sure", 1.0);

        // Assert
        suggestion.Confidence.Should().Be(1.0);
    }

    #endregion

    #region CodeAnalysisResult Tests

    [Fact]
    public void CodeAnalysisResult_WithNoErrors_IsValid()
    {
        // Act
        var result = new CodeAnalysisResult(
            new[] { "MyClass" },
            new[] { "MyMethod" },
            Array.Empty<Diagnostic>());

        // Assert
        result.IsValid.Should().BeTrue();
        result.Classes.Should().Contain("MyClass");
        result.Methods.Should().Contain("MyMethod");
        result.Findings.Should().BeEmpty();
    }

    [Fact]
    public void CodeAnalysisResult_WithFindings_PreservesFindings()
    {
        // Act
        var result = new CodeAnalysisResult(
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<Diagnostic>(),
            new[] { "Finding 1", "Finding 2" });

        // Assert
        result.Findings.Should().HaveCount(2);
        result.Findings.Should().Contain("Finding 1");
    }

    [Fact]
    public void CodeAnalysisResult_NullFindings_DefaultsToEmpty()
    {
        // Act
        var result = new CodeAnalysisResult(
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<Diagnostic>(),
            null);

        // Assert
        result.Findings.Should().BeEmpty();
    }

    #endregion

    #region MockMcpServer Tests

    [Fact]
    public void MockMcpServer_ListTools_ReturnsTwoTools()
    {
        // Act
        var tools = MockMcpServer.ListTools();

        // Assert
        tools.Should().HaveCount(2);
        tools.Select(t => t.Name).Should().Contain("dsl_suggestion");
        tools.Select(t => t.Name).Should().Contain("code_analysis");
    }

    [Fact]
    public async Task MockMcpServer_ExecuteTool_ReturnsSuccessResult()
    {
        // Act
        var result = await MockMcpServer.ExecuteTool("dsl_suggestion", new { });

        // Assert
        result.Success.Should().BeTrue();
        result.Result.Should().Contain("Executed dsl_suggestion");
    }

    [Fact]
    public async Task MockMcpServer_ExecuteTool_IncludesToolNameInResult()
    {
        // Act
        var result = await MockMcpServer.ExecuteTool("code_analysis", new { code = "test" });

        // Assert
        result.Result.Should().Contain("code_analysis");
    }

    #endregion

    #region GitHubToolExtensions Tests

    [Fact]
    public void WithGitHubTools_RegistersAllGitHubTools()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var withGithub = registry.WithGitHubTools("fake-token", "owner", "repo");

        // Assert
        withGithub.Count.Should().Be(8);
        withGithub.Contains("github_read_issue").Should().BeTrue();
        withGithub.Contains("github_create_issue").Should().BeTrue();
        withGithub.Contains("github_update_issue").Should().BeTrue();
        withGithub.Contains("github_create_pr").Should().BeTrue();
        withGithub.Contains("github_add_comment").Should().BeTrue();
        withGithub.Contains("github_manage_labels").Should().BeTrue();
        withGithub.Contains("github_search").Should().BeTrue();
        withGithub.Contains("github_scope_lock").Should().BeTrue();
    }

    [Fact]
    public void WithGitHubTools_ChainedWithOtherTools_PreservesAll()
    {
        // Arrange
        var registry = new ToolRegistry()
            .WithTool(new MathTool());

        // Act
        var withAll = registry.WithGitHubTools("token", "owner", "repo");

        // Assert
        withAll.Contains("math").Should().BeTrue();
        withAll.Count.Should().Be(9); // 1 math + 8 github
    }

    #endregion

    #region RetrievalArgs Tests

    [Fact]
    public void RetrievalArgs_DefaultValues_SetCorrectly()
    {
        // Act
        var args = new RetrievalArgs();

        // Assert
        args.Q.Should().BeEmpty();
        args.K.Should().Be(3);
    }

    [Fact]
    public void RetrievalArgs_SetProperties_Persisted()
    {
        // Act
        var args = new RetrievalArgs { Q = "search query", K = 10 };

        // Assert
        args.Q.Should().Be("search query");
        args.K.Should().Be(10);
    }

    #endregion

    #region GitHub Args Tests

    [Fact]
    public void GitHubCommentArgs_DefaultValues()
    {
        var args = new GitHubCommentArgs();
        args.IssueNumber.Should().Be(0);
        args.Body.Should().BeEmpty();
    }

    [Fact]
    public void GitHubIssueCreateArgs_DefaultValues()
    {
        var args = new GitHubIssueCreateArgs();
        args.Title.Should().BeEmpty();
        args.Body.Should().BeNull();
        args.Labels.Should().BeNull();
        args.Assignees.Should().BeNull();
    }

    [Fact]
    public void GitHubSearchArgs_DefaultValues()
    {
        var args = new GitHubSearchArgs();
        args.Query.Should().BeEmpty();
        args.Type.Should().BeNull();
        args.MaxResults.Should().BeNull();
    }

    #endregion
}
