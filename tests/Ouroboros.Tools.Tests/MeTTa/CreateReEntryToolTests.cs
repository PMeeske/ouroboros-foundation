// <copyright file="CreateReEntryToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using System.Text.Json;

namespace Ouroboros.Tools.Tests.MeTTa;

/// <summary>
/// Comprehensive tests for <see cref="CreateReEntryTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class CreateReEntryToolTests
{
    private readonly IAtomSpace _atomSpace;
    private readonly FormMeTTaBridge _bridge;

    public CreateReEntryToolTests()
    {
        _atomSpace = new AtomSpace();
        _bridge = new FormMeTTaBridge(_atomSpace);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidBridge_DoesNotThrow()
    {
        // Act
        var tool = new CreateReEntryTool(_bridge);

        // Assert
        tool.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullBridge_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new CreateReEntryTool(null!);

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
        var tool = new CreateReEntryTool(_bridge);

        // Assert
        tool.Name.Should().Be("lof_create_reentry");
    }

    [Fact]
    public void Description_IsNotNullOrEmpty()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);

        // Assert
        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Description_ContainsReEntry()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("re-entry") || d.Contains("Re-entry") || d.Contains("self-referential"));
    }

    [Fact]
    public void Description_MentionsImaginary()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);

        // Assert
        tool.Description.Should().Match(d => d.Contains("Imaginary") || d.Contains("oscillation") || d.Contains("paradox"));
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);

        // Assert
        tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void JsonSchema_ContainsContextProperty()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("context");
    }

    [Fact]
    public void JsonSchema_ContainsDepthProperty()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("depth");
    }

    [Fact]
    public void JsonSchema_IsValidJson()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);

        // Act
        Action act = () => JsonDocument.Parse(tool.JsonSchema!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void JsonSchema_ContextIsRequired()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);

        // Assert
        tool.JsonSchema.Should().Contain("\"required\"");
        tool.JsonSchema.Should().Contain("\"context\"");
    }

    [Fact]
    public void Tool_ImplementsITool()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);

        // Assert
        tool.Should().BeAssignableTo<ITool>();
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithValidContext_ReturnsSuccess()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);
        string input = """{"context": "test-reentry"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Re-entry created");
        result.Value.Should().Contain("test-reentry");
    }

    [Fact]
    public async Task InvokeAsync_WithValidContext_ReturnsImaginaryState()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);
        string input = """{"context": "imaginary-test"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Imaginary");
    }

    [Fact]
    public async Task InvokeAsync_WithMissingContext_UsesDefault()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);
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
        var tool = new CreateReEntryTool(_bridge);
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
        var tool = new CreateReEntryTool(_bridge);
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
        var tool = new CreateReEntryTool(_bridge);
        string input = "{broken: json}";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AfterDrawDistinction_OverridesToImaginary()
    {
        // Arrange - first draw a distinction (Mark), then create re-entry (Imaginary)
        _bridge.DrawDistinction("override-test");
        var tool = new CreateReEntryTool(_bridge);
        string input = """{"context": "override-test"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Imaginary");
    }

    [Fact]
    public async Task InvokeAsync_UpdatesBridgeStateToImaginary()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);
        string input = """{"context": "state-check"}""";

        // Act
        await tool.InvokeAsync(input);

        // Assert
        Form state = _bridge.GetFormState("state-check");
        state.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_RespectsToken()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);
        string input = """{"context": "cancel-test"}""";
        using var cts = new CancellationTokenSource();

        // Act
        var result = await tool.InvokeAsync(input, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_CreatingReEntryTwice_BothSucceed()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);
        string input = """{"context": "twice"}""";

        // Act
        var result1 = await tool.InvokeAsync(input);
        var result2 = await tool.InvokeAsync(input);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithDepthField_ReturnsSuccess()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);
        string input = """{"context": "depth-test", "depth": 3}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_SelfReferentialDescription_ContainsExpectedText()
    {
        // Arrange
        var tool = new CreateReEntryTool(_bridge);
        string input = """{"context": "self-ref"}""";

        // Act
        var result = await tool.InvokeAsync(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("self-referential");
    }

    #endregion
}
