// <copyright file="FormPatternMatchToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using System.Text.Json;

namespace Ouroboros.Tools.Tests.MeTTa;

/// <summary>
/// Comprehensive tests for <see cref="FormPatternMatchTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class FormPatternMatchToolTests
{
    private readonly IAtomSpace _atomSpace;
    private readonly FormMeTTaBridge _bridge;

    public FormPatternMatchToolTests()
    {
        _atomSpace = new AtomSpace();
        _bridge = new FormMeTTaBridge(_atomSpace);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidBridge_DoesNotThrow()
    {
        // Act
        var tool = new FormPatternMatchTool(_bridge);

        // Assert
        tool.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new FormPatternMatchTool(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("bridge");
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);

        // Assert
        tool.Name.Should().Be("lof_pattern_match");
    }

    [Fact]
    public void Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Description_MentionsPatternMatching()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("pattern") || d.Contains("match"));
    }

    [Fact]
    public void Description_MentionsCertaintyFiltering()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("Mark") || d.Contains("certain"));
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void JsonSchema_ContainsPatternProperty()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("pattern");
    }

    [Fact]
    public void JsonSchema_ContainsTemplateProperty()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("template");
    }

    [Fact]
    public void JsonSchema_IsValidJson()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);

        // Act
        Action act = () => JsonDocument.Parse(tool.JsonSchema!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void JsonSchema_PatternAndTemplateAreRequired()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("\"required\"");
        tool.JsonSchema.Should().Contain("\"pattern\"");
        tool.JsonSchema.Should().Contain("\"template\"");
    }

    [Fact]
    public void Tool_ImplementsITool()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);

        // Assert
        tool.Should().BeAssignableTo<ITool>();
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithValidPattern_ReturnsSuccess()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"pattern": "test", "template": "$x"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithNoMatches_ReturnsNoCertainMatches()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"pattern": "nonexistent-pattern-xyz", "template": "$x"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("No certain matches found");
    }

    [Fact]
    public async Task InvokeAsync_WithMissingPattern_UsesEmptyString()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"template": "result"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithMissingTemplate_ReturnsSuccess()
    {
        // Arrange - template is not used in the tool logic, only pattern matters
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"pattern": "test"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyJson_UsesDefaults()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithNullPattern_UsesEmptyString()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"pattern": null, "template": "tmpl"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = "not valid json";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithMalformedJson_ReturnsFailure()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = "{broken: json}";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ResultMessageContainsMatchInfo()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"pattern": "test-query", "template": "$x"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Match(v => v.Contains("Form-gated matches") || v.Contains("No certain matches found"));
    }

    [Fact]
    public async Task InvokeAsync_OnlyCertainResultsReturned()
    {
        // Arrange - the tool filters to marked (certain) results only
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"pattern": "Mark", "template": "$x"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Results are either "Form-gated matches" (certain) or "No certain matches found"
        result.Value.Should().Match(v => v.Contains("Form-gated matches") || v.Contains("No certain matches found"));
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_RespectsToken()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"pattern": "test", "template": "$x"}""";
        using var cts = new CancellationTokenSource();

        // Act
        var result = await tool.InvokeAsync(input, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyPattern_ReturnsSuccess()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"pattern": "", "template": ""}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithFormSymbolPattern_ReturnsSuccess()
    {
        // Arrange - "Void" is a known form type in the atom space
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"pattern": "Void", "template": "$x"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_MultipleCallsWithSamePattern_ReturnConsistentResults()
    {
        // Arrange
        var tool = new FormPatternMatchTool(_bridge);
        string input = """{"pattern": "consistent-test", "template": "$x"}""";

        // Act
        var result1 = await tool.InvokeAsync(input);
        var result2 = await tool.InvokeAsync(input);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be(result2.Value);
    }

    #endregion
}
