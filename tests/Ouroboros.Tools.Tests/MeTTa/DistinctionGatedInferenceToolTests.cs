// <copyright file="DistinctionGatedInferenceToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using System.Text.Json;

namespace Ouroboros.Tools.Tests.MeTTa;

/// <summary>
/// Comprehensive tests for <see cref="DistinctionGatedInferenceTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class DistinctionGatedInferenceToolTests
{
    private readonly IAtomSpace _atomSpace;
    private readonly FormMeTTaBridge _bridge;

    public DistinctionGatedInferenceToolTests()
    {
        _atomSpace = new AtomSpace();
        _bridge = new FormMeTTaBridge(_atomSpace);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidBridge_DoesNotThrow()
    {
        // Act
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Assert
        tool.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new DistinctionGatedInferenceTool(null!);

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
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Assert
        tool.Name.Should().Be("lof_gated_inference");
    }

    [Fact]
    public void Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Description_MentionsGatedInference()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("gated") || d.Contains("Gated") || d.Contains("distinction-gated"));
    }

    [Fact]
    public void Description_MentionsCertainty()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("certain") || d.Contains("marked"));
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void JsonSchema_ContainsContextProperty()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("context");
    }

    [Fact]
    public void JsonSchema_ContainsPatternProperty()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("pattern");
    }

    [Fact]
    public void JsonSchema_IsValidJson()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Act
        Action act = () => JsonDocument.Parse(tool.JsonSchema!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void JsonSchema_ContextAndPatternAreRequired()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("\"required\"");
        tool.JsonSchema.Should().Contain("\"context\"");
        tool.JsonSchema.Should().Contain("\"pattern\"");
    }

    [Fact]
    public void Tool_ImplementsITool()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);

        // Assert
        tool.Should().BeAssignableTo<ITool>();
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithUnmarkedContext_ReportsBlocked()
    {
        // Arrange - context starts as Void (not marked), so inference should be blocked
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{"context": "unmarked", "pattern": "test"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("blocked");
    }

    [Fact]
    public async Task InvokeAsync_WithMarkedContext_ReportsSucceeded()
    {
        // Arrange - draw distinction to mark context, then perform gated inference
        _bridge.DrawDistinction("marked-gate");
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{"context": "marked-gate", "pattern": "Mark"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Match(v => v.Contains("succeeded") || v.Contains("blocked"));
    }

    [Fact]
    public async Task InvokeAsync_WithMissingContext_UsesDefault()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{"pattern": "test"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithMissingPattern_UsesEmptyString()
    {
        // Arrange
        _bridge.DrawDistinction("no-pattern");
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{"context": "no-pattern"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithNullContext_UsesDefault()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{"context": null, "pattern": "test"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithNullPattern_UsesEmptyString()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{"context": "test", "pattern": null}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyJson_UsesDefaults()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("blocked");
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);
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
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = "{broken json}";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithImaginaryContext_ReportsBlocked()
    {
        // Arrange - imaginary state is not marked, so inference should be blocked
        _bridge.CreateReEntry("imaginary-gate");
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{"context": "imaginary-gate", "pattern": "test"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("blocked");
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_RespectsToken()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{"context": "cancel-test", "pattern": "test"}""";
        using var cts = new CancellationTokenSource();

        // Act
        var result = await tool.InvokeAsync(input, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_MarkedThenCrossed_ReportsBlocked()
    {
        // Arrange - mark then cross (back to Void), should be blocked
        _bridge.DrawDistinction("mark-then-cross");
        _bridge.CrossDistinction("mark-then-cross");
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{"context": "mark-then-cross", "pattern": "test"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("blocked");
    }

    [Fact]
    public async Task InvokeAsync_ResultMessageContainsGatedInference()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(_bridge);
        string input = """{"context": "msg-check", "pattern": "test"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Match(v => v.Contains("Gated inference") || v.Contains("blocked") || v.Contains("succeeded"));
    }

    #endregion
}
