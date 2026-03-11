// <copyright file="CrossDistinctionToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using System.Text.Json;

namespace Ouroboros.Tools.Tests.MeTTa;

/// <summary>
/// Comprehensive tests for <see cref="CrossDistinctionTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class CrossDistinctionToolTests
{
    private readonly IAtomSpace _atomSpace;
    private readonly FormMeTTaBridge _bridge;

    public CrossDistinctionToolTests()
    {
        _atomSpace = new AtomSpace();
        _bridge = new FormMeTTaBridge(_atomSpace);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidBridge_DoesNotThrow()
    {
        // Act
        var tool = new CrossDistinctionTool(_bridge);

        // Assert
        tool.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new CrossDistinctionTool(null!);

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
        var tool = new CrossDistinctionTool(_bridge);

        // Assert
        tool.Name.Should().Be("lof_cross_distinction");
    }

    [Fact]
    public void Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Description_ContainsCrossing()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("Cross") || d.Contains("cross") || d.Contains("negat"));
    }

    [Fact]
    public void Description_MentionsLawOfCrossing()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("Law of Crossing") || d.Contains("Double crossing") || d.Contains("cancels"));
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void JsonSchema_ContainsContextProperty()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("context");
    }

    [Fact]
    public void JsonSchema_IsValidJson()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);

        // Act
        Action act = () => JsonDocument.Parse(tool.JsonSchema!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void JsonSchema_ContextIsRequired()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("\"required\"");
        tool.JsonSchema.Should().Contain("\"context\"");
    }

    [Fact]
    public void Tool_ImplementsITool()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);

        // Assert
        tool.Should().BeAssignableTo<ITool>();
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithValidContext_ReturnsSuccess()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);
        string input = """{"context": "test-cross"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Distinction crossed");
    }

    [Fact]
    public async Task InvokeAsync_CrossingVoidContext_ProducesMark()
    {
        // Arrange - context starts as Void, crossing Void -> Mark
        var tool = new CrossDistinctionTool(_bridge);
        string input = """{"context": "void-cross"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Mark (certain)");
    }

    [Fact]
    public async Task InvokeAsync_CrossingMarkedContext_ProducesVoid()
    {
        // Arrange - first draw distinction to get Mark, then cross
        _bridge.DrawDistinction("marked-cross");
        var tool = new CrossDistinctionTool(_bridge);
        string input = """{"context": "marked-cross"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Void (negated)");
    }

    [Fact]
    public async Task InvokeAsync_DoubleCrossing_RestoresOriginalState()
    {
        // Arrange - Void -> cross -> Mark -> cross -> Void (Law of Crossing)
        var tool = new CrossDistinctionTool(_bridge);
        string context = "double-cross";

        // Act
        var result1 = await tool.InvokeAsync($"""{{ "context": "{context}" }}""");
        var result2 = await tool.InvokeAsync($"""{{ "context": "{context}" }}""");

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().Contain("Mark (certain)");
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Contain("Void (negated)");
    }

    [Fact]
    public async Task InvokeAsync_WithMissingContext_UsesDefault()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);
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
        var tool = new CrossDistinctionTool(_bridge);
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
        var tool = new CrossDistinctionTool(_bridge);
        string input = "invalid json";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithMalformedJson_ReturnsFailure()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);
        string input = "{ not: json }";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_CrossingImaginaryContext_StaysImaginary()
    {
        // Arrange - create re-entry (Imaginary), then cross
        _bridge.CreateReEntry("imaginary-cross");
        var tool = new CrossDistinctionTool(_bridge);
        string input = """{"context": "imaginary-cross"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Imaginary (uncertain)");
    }

    [Fact]
    public async Task InvokeAsync_UpdatesBridgeState()
    {
        // Arrange - start with Void, cross to Mark
        var tool = new CrossDistinctionTool(_bridge);
        string input = """{"context": "state-check"}""";

        // Act
        await tool.InvokeAsync(input);

        // Assert
        Form state = _bridge.GetFormState("state-check");
        state.IsMarked().Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_RespectsToken()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);
        string input = """{"context": "cancel-test"}""";
        using var cts = new CancellationTokenSource();

        // Act
        var result = await tool.InvokeAsync(input, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyContextString_Succeeds()
    {
        // Arrange
        var tool = new CrossDistinctionTool(_bridge);
        string input = """{"context": ""}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
