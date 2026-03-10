// <copyright file="EvaluateCertaintyToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using System.Text.Json;

namespace Ouroboros.Tools.Tests.MeTTa;

/// <summary>
/// Comprehensive tests for <see cref="EvaluateCertaintyTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class EvaluateCertaintyToolTests
{
    private readonly IAtomSpace _atomSpace;
    private readonly FormMeTTaBridge _bridge;

    public EvaluateCertaintyToolTests()
    {
        _atomSpace = new AtomSpace();
        _bridge = new FormMeTTaBridge(_atomSpace);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidBridge_DoesNotThrow()
    {
        // Act
        var tool = new EvaluateCertaintyTool(_bridge);

        // Assert
        tool.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new EvaluateCertaintyTool(null!);

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
        var tool = new EvaluateCertaintyTool(_bridge);

        // Assert
        tool.Name.Should().Be("lof_evaluate_certainty");
    }

    [Fact]
    public void Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Description_MentionsCertainty()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("certainty") || d.Contains("certain"));
    }

    [Fact]
    public void Description_MentionsFormStates()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("Mark") || d.Contains("Void") || d.Contains("Imaginary"));
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void JsonSchema_ContainsExpressionProperty()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("expression");
    }

    [Fact]
    public void JsonSchema_IsValidJson()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);

        // Act
        Action act = () => JsonDocument.Parse(tool.JsonSchema!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void JsonSchema_ExpressionIsRequired()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("\"required\"");
        tool.JsonSchema.Should().Contain("\"expression\"");
    }

    [Fact]
    public void Tool_ImplementsITool()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);

        // Assert
        tool.Should().BeAssignableTo<ITool>();
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithValidExpression_ReturnsSuccess()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = """{"expression": "test-expr"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Certainty of");
    }

    [Fact]
    public async Task InvokeAsync_WithMarkExpression_ReturnsCertainAffirmed()
    {
        // Arrange - "Mark" is a known form symbol
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = """{"expression": "Mark"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Certain (affirmed)");
    }

    [Fact]
    public async Task InvokeAsync_WithVoidExpression_ReturnsCertainNegated()
    {
        // Arrange - "Void" is a known form symbol
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = """{"expression": "Void"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Certain (negated)");
    }

    [Fact]
    public async Task InvokeAsync_WithImaginaryExpression_ReturnsUncertain()
    {
        // Arrange - "Imaginary" is a known form symbol
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = """{"expression": "Imaginary"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Uncertain");
    }

    [Fact]
    public async Task InvokeAsync_WithMissingExpression_UsesEmptyString()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = """{}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithNullExpressionValue_UsesEmptyString()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = """{"expression": null}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);
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
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = "{bad: json}";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithUnknownExpression_ReturnsSuccess()
    {
        // Arrange - unknown expressions are evaluated to Void by the interpreter
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = """{"expression": "completely-unknown-symbol"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Certainty of");
    }

    [Fact]
    public async Task InvokeAsync_ResultContainsExpressionName()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = """{"expression": "my-test-expression"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("my-test-expression");
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_RespectsToken()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = """{"expression": "test"}""";
        using var cts = new CancellationTokenSource();

        // Act
        var result = await tool.InvokeAsync(input, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyExpression_ReturnsSuccess()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);
        string input = """{"expression": ""}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ResultAlwaysContainsCertaintyDescription()
    {
        // Arrange
        var tool = new EvaluateCertaintyTool(_bridge);

        // Act
        var result = await tool.InvokeAsync("""{"expression": "anything"}""");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Match(v => v.Contains("Certain (affirmed)") || v.Contains("Certain (negated)") || v.Contains("Uncertain (imaginary/paradoxical)"));
    }

    #endregion
}
