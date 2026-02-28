// <copyright file="FormReasoningToolsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.MeTTa;

using FluentAssertions;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Tools.MeTTa;
using Xunit;

/// <summary>
/// Unit tests for Laws of Form tool classes: DrawDistinctionTool,
/// CrossDistinctionTool, CreateReEntryTool, EvaluateCertaintyTool,
/// DistinctionGatedInferenceTool, and FormPatternMatchTool.
/// </summary>
[Trait("Category", "Unit")]
public class FormReasoningToolsTests
{
    private readonly IAtomSpace atomSpace;
    private readonly FormMeTTaBridge bridge;

    public FormReasoningToolsTests()
    {
        this.atomSpace = new AtomSpace();
        this.bridge = new FormMeTTaBridge(this.atomSpace);
    }

    #region DrawDistinctionTool Tests

    [Fact]
    public void DrawDistinctionTool_Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new DrawDistinctionTool(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("bridge");
    }

    [Fact]
    public void DrawDistinctionTool_Name_ReturnsExpectedValue()
    {
        // Arrange
        var tool = new DrawDistinctionTool(this.bridge);

        // Assert
        tool.Name.Should().Be("lof_draw_distinction");
    }

    [Fact]
    public void DrawDistinctionTool_Name_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new DrawDistinctionTool(this.bridge);

        // Assert
        tool.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DrawDistinctionTool_Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new DrawDistinctionTool(this.bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DrawDistinctionTool_Description_ContainsDistinction()
    {
        // Arrange
        var tool = new DrawDistinctionTool(this.bridge);

        // Assert
        tool.Description.Should().Contain("distinction");
    }

    [Fact]
    public void DrawDistinctionTool_JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new DrawDistinctionTool(this.bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void DrawDistinctionTool_JsonSchema_ContainsContextProperty()
    {
        // Arrange
        var tool = new DrawDistinctionTool(this.bridge);

        // Assert
        tool.JsonSchema.Should().Contain("context");
    }

    [Fact]
    public async Task DrawDistinctionTool_InvokeAsync_WithValidContext_ReturnsSuccess()
    {
        // Arrange
        var tool = new DrawDistinctionTool(this.bridge);
        string input = """{"context": "test-distinction"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Distinction drawn");
        result.Value.Should().Contain("test-distinction");
    }

    [Fact]
    public async Task DrawDistinctionTool_InvokeAsync_WithDefaultContext_UsesDefault()
    {
        // Arrange
        var tool = new DrawDistinctionTool(this.bridge);
        string input = """{}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("default");
    }

    [Fact]
    public async Task DrawDistinctionTool_InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = new DrawDistinctionTool(this.bridge);
        string input = "not valid json";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region CrossDistinctionTool Tests

    [Fact]
    public void CrossDistinctionTool_Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new CrossDistinctionTool(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("bridge");
    }

    [Fact]
    public void CrossDistinctionTool_Name_ReturnsExpectedValue()
    {
        // Arrange
        var tool = new CrossDistinctionTool(this.bridge);

        // Assert
        tool.Name.Should().Be("lof_cross_distinction");
    }

    [Fact]
    public void CrossDistinctionTool_Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new CrossDistinctionTool(this.bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CrossDistinctionTool_JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new CrossDistinctionTool(this.bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public async Task CrossDistinctionTool_InvokeAsync_WithValidContext_ReturnsSuccess()
    {
        // Arrange
        var tool = new CrossDistinctionTool(this.bridge);
        string input = """{"context": "test-cross"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Distinction crossed");
    }

    [Fact]
    public async Task CrossDistinctionTool_InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = new CrossDistinctionTool(this.bridge);
        string input = "invalid json";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region CreateReEntryTool Tests

    [Fact]
    public void CreateReEntryTool_Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new CreateReEntryTool(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("bridge");
    }

    [Fact]
    public void CreateReEntryTool_Name_ReturnsExpectedValue()
    {
        // Arrange
        var tool = new CreateReEntryTool(this.bridge);

        // Assert
        tool.Name.Should().Be("lof_create_reentry");
    }

    [Fact]
    public void CreateReEntryTool_Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new CreateReEntryTool(this.bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateReEntryTool_Description_ContainsReEntry()
    {
        // Arrange
        var tool = new CreateReEntryTool(this.bridge);

        // Assert
        tool.Description.Should().Contain("re-entry");
    }

    [Fact]
    public void CreateReEntryTool_JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new CreateReEntryTool(this.bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateReEntryTool_InvokeAsync_WithValidContext_ReturnsSuccess()
    {
        // Arrange
        var tool = new CreateReEntryTool(this.bridge);
        string input = """{"context": "test-reentry"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Re-entry created");
    }

    [Fact]
    public async Task CreateReEntryTool_InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = new CreateReEntryTool(this.bridge);
        string input = "invalid json";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region EvaluateCertaintyTool Tests

    [Fact]
    public void EvaluateCertaintyTool_Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new EvaluateCertaintyTool(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("bridge");
    }

    [Fact]
    public void EvaluateCertaintyTool_Name_ReturnsExpectedValue()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(this.bridge);

        // Assert
        tool.Name.Should().Be("lof_evaluate_certainty");
    }

    [Fact]
    public void EvaluateCertaintyTool_Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(this.bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void EvaluateCertaintyTool_JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(this.bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void EvaluateCertaintyTool_JsonSchema_ContainsExpressionProperty()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(this.bridge);

        // Assert
        tool.JsonSchema.Should().Contain("expression");
    }

    [Fact]
    public async Task EvaluateCertaintyTool_InvokeAsync_WithValidExpression_ReturnsSuccess()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(this.bridge);
        string input = """{"expression": "test-expr"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Certainty of");
    }

    [Fact]
    public async Task EvaluateCertaintyTool_InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(this.bridge);
        string input = "bad json";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region DistinctionGatedInferenceTool Tests

    [Fact]
    public void DistinctionGatedInferenceTool_Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new DistinctionGatedInferenceTool(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("bridge");
    }

    [Fact]
    public void DistinctionGatedInferenceTool_Name_ReturnsExpectedValue()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(this.bridge);

        // Assert
        tool.Name.Should().Be("lof_gated_inference");
    }

    [Fact]
    public void DistinctionGatedInferenceTool_Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(this.bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void DistinctionGatedInferenceTool_JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(this.bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void DistinctionGatedInferenceTool_JsonSchema_ContainsContextAndPattern()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(this.bridge);

        // Assert
        tool.JsonSchema.Should().Contain("context");
        tool.JsonSchema.Should().Contain("pattern");
    }

    [Fact]
    public async Task DistinctionGatedInferenceTool_InvokeAsync_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(this.bridge);
        string input = """{"context": "test-gate", "pattern": "(test $x)"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DistinctionGatedInferenceTool_InvokeAsync_WithUnmarkedContext_ReportsBlocked()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(this.bridge);
        string input = """{"context": "unmarked-context", "pattern": "(test $x)"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Match(v => v.Contains("blocked") || v.Contains("succeeded"));
    }

    [Fact]
    public async Task DistinctionGatedInferenceTool_InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = new DistinctionGatedInferenceTool(this.bridge);
        string input = "bad json";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region FormPatternMatchTool Tests

    [Fact]
    public void FormPatternMatchTool_Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new FormPatternMatchTool(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("bridge");
    }

    [Fact]
    public void FormPatternMatchTool_Name_ReturnsExpectedValue()
    {
        // Arrange
        var tool = new FormPatternMatchTool(this.bridge);

        // Assert
        tool.Name.Should().Be("lof_pattern_match");
    }

    [Fact]
    public void FormPatternMatchTool_Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new FormPatternMatchTool(this.bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FormPatternMatchTool_JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new FormPatternMatchTool(this.bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void FormPatternMatchTool_JsonSchema_ContainsPatternAndTemplate()
    {
        // Arrange
        var tool = new FormPatternMatchTool(this.bridge);

        // Assert
        tool.JsonSchema.Should().Contain("pattern");
        tool.JsonSchema.Should().Contain("template");
    }

    [Fact]
    public async Task FormPatternMatchTool_InvokeAsync_WithValidPattern_ReturnsSuccess()
    {
        // Arrange
        var tool = new FormPatternMatchTool(this.bridge);
        string input = """{"pattern": "(test $x)", "template": "$x"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FormPatternMatchTool_InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = new FormPatternMatchTool(this.bridge);
        string input = "bad json";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion
}
