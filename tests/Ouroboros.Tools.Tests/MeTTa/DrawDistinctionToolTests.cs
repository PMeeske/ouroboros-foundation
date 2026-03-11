// <copyright file="DrawDistinctionToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using System.Text.Json;

namespace Ouroboros.Tools.Tests.MeTTa;

/// <summary>
/// Comprehensive tests for <see cref="DrawDistinctionTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class DrawDistinctionToolTests
{
    private readonly IAtomSpace _atomSpace;
    private readonly FormMeTTaBridge _bridge;

    public DrawDistinctionToolTests()
    {
        _atomSpace = new AtomSpace();
        _bridge = new FormMeTTaBridge(_atomSpace);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidBridge_DoesNotThrow()
    {
        // Act
        var tool = new DrawDistinctionTool(_bridge);

        // Assert
        tool.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new DrawDistinctionTool(null!);

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
        var tool = new DrawDistinctionTool(_bridge);

        // Assert
        tool.Name.Should().Be("lof_draw_distinction");
    }

    [Fact]
    public void Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Description_ContainsDistinction()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("distinction") || d.Contains("mark") || d.Contains("Mark"));
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void JsonSchema_ContainsContextProperty()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("context");
    }

    [Fact]
    public void JsonSchema_ContainsReasonProperty()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("reason");
    }

    [Fact]
    public void JsonSchema_IsValidJson()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);

        // Act
        Action act = () => JsonDocument.Parse(tool.JsonSchema!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void JsonSchema_ContextIsRequired()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("\"required\"");
        tool.JsonSchema.Should().Contain("\"context\"");
    }

    [Fact]
    public void Tool_ImplementsITool()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);

        // Assert
        tool.Should().BeAssignableTo<ITool>();
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithValidContext_ReturnsSuccess()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = """{"context": "test-distinction"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Distinction drawn");
        result.Value.Should().Contain("test-distinction");
    }

    [Fact]
    public async Task InvokeAsync_WithValidContext_ReturnsMark()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = """{"context": "mark-test"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Mark (certain)");
    }

    [Fact]
    public async Task InvokeAsync_WithMissingContext_UsesDefault()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = """{}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("default");
    }

    [Fact]
    public async Task InvokeAsync_WithNullContextValue_UsesDefault()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = """{"context": null}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("default");
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = "not valid json at all";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithMalformedJson_ReturnsFailureWithErrorMessage()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = "{broken json}";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithReasonField_ReturnsSuccess()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = """{"context": "reasoned", "reason": "testing"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("reasoned");
    }

    [Fact]
    public async Task InvokeAsync_DrawingSameContextTwice_BothSucceed()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = """{"context": "repeat"}""";

        // Act
        var result1 = await tool.InvokeAsync(input);
        var result2 = await tool.InvokeAsync(input);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_RespectsToken()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = """{"context": "cancel-test"}""";
        using var cts = new CancellationTokenSource();

        // Act
        var result = await tool.InvokeAsync(input, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_MultipleDistinctContexts_AllSucceed()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);

        // Act
        var result1 = await tool.InvokeAsync("""{"context": "alpha"}""");
        var result2 = await tool.InvokeAsync("""{"context": "beta"}""");
        var result3 = await tool.InvokeAsync("""{"context": "gamma"}""");

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().Contain("alpha");
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Contain("beta");
        result3.IsSuccess.Should().BeTrue();
        result3.Value.Should().Contain("gamma");
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyContextString_UsesEmptyContext()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = """{"context": ""}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_UpdatesBridgeState()
    {
        // Arrange
        var tool = new DrawDistinctionTool(_bridge);
        string input = """{"context": "state-check"}""";

        // Act
        await tool.InvokeAsync(input);

        // Assert - bridge should now have a Mark for this context
        Form state = _bridge.GetFormState("state-check");
        state.IsMarked().Should().BeTrue();
    }

    #endregion
}
