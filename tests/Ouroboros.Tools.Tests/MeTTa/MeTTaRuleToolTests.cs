// <copyright file="MeTTaRuleToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.MeTTa;

using FluentAssertions;
using Moq;
using Ouroboros.Tools;
using Ouroboros.Tools.MeTTa;
using Xunit;

/// <summary>
/// Unit tests for <see cref="MeTTaRuleTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class MeTTaRuleToolTests
{
    private readonly Mock<IMeTTaEngine> mockEngine = new();
    private readonly MeTTaRuleTool tool;

    public MeTTaRuleToolTests()
    {
        this.tool = new MeTTaRuleTool(this.mockEngine.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullEngine_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new MeTTaRuleTool(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("engine");
    }

    #endregion

    #region ITool Interface Tests

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Assert
        this.tool.Name.Should().Be("metta_rule");
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
    public void Description_ContainsMeTTaKeyword()
    {
        // Assert
        this.tool.Description.Should().Contain("MeTTa");
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        // Assert
        this.tool.JsonSchema.Should().NotBeNull();
    }

    [Fact]
    public void JsonSchema_ContainsRuleProperty()
    {
        // Assert
        this.tool.JsonSchema.Should().Contain("rule");
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithEmptyInput_ReturnsFailure()
    {
        // Act
        var result = await this.tool.InvokeAsync(string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespace_ReturnsFailure()
    {
        // Act
        var result = await this.tool.InvokeAsync("   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task InvokeAsync_WithDirectRule_DelegatesToEngine()
    {
        // Arrange
        string rule = "(implies (parent $x $y) (ancestor $x $y))";
        this.mockEngine
            .Setup(e => e.ApplyRuleAsync(rule, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("Rule applied"));

        // Act
        var result = await this.tool.InvokeAsync(rule);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Rule applied");
        this.mockEngine.Verify(
            e => e.ApplyRuleAsync(rule, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithJsonInput_ExtractsRuleFromJson()
    {
        // Arrange
        string jsonInput = """{"rule": "(implies (parent $x $y) (ancestor $x $y))"}""";
        string expectedRule = "(implies (parent $x $y) (ancestor $x $y))";
        this.mockEngine
            .Setup(e => e.ApplyRuleAsync(expectedRule, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("Rule applied"));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.mockEngine.Verify(
            e => e.ApplyRuleAsync(expectedRule, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenEngineReturnsFailure_PropagatesFailure()
    {
        // Arrange
        string rule = "(invalid-rule)";
        this.mockEngine
            .Setup(e => e.ApplyRuleAsync(rule, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Failure("Invalid rule format"));

        // Act
        var result = await this.tool.InvokeAsync(rule);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid rule format");
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_PassesTokenToEngine()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        string rule = "(implies (A $x) (B $x))";
        this.mockEngine
            .Setup(e => e.ApplyRuleAsync(rule, cts.Token))
            .ReturnsAsync(Result<string, string>.Success("Applied"));

        // Act
        var result = await this.tool.InvokeAsync(rule, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.mockEngine.Verify(
            e => e.ApplyRuleAsync(rule, cts.Token),
            Times.Once);
    }

    #endregion
}
