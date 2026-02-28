// <copyright file="MeTTaFactToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.MeTTa;

using FluentAssertions;
using Moq;
using Ouroboros.Abstractions;
using Ouroboros.Tools;
using Ouroboros.Tools.MeTTa;
using Xunit;

/// <summary>
/// Unit tests for <see cref="MeTTaFactTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class MeTTaFactToolTests
{
    private readonly Mock<IMeTTaEngine> mockEngine = new();
    private readonly MeTTaFactTool tool;

    public MeTTaFactToolTests()
    {
        this.tool = new MeTTaFactTool(this.mockEngine.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullEngine_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new MeTTaFactTool(null!);

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
        this.tool.Name.Should().Be("metta_add_fact");
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
    public void JsonSchema_ContainsFactProperty()
    {
        // Assert
        this.tool.JsonSchema.Should().Contain("fact");
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
    public async Task InvokeAsync_WithDirectFact_DelegatesToEngine()
    {
        // Arrange
        string fact = "(parent John Mary)";
        this.mockEngine
            .Setup(e => e.AddFactAsync(fact, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(default));

        // Act
        var result = await this.tool.InvokeAsync(fact);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("added successfully");
        this.mockEngine.Verify(
            e => e.AddFactAsync(fact, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithJsonInput_ExtractsFactFromJson()
    {
        // Arrange
        string jsonInput = """{"fact": "(likes Alice Bob)"}""";
        string expectedFact = "(likes Alice Bob)";
        this.mockEngine
            .Setup(e => e.AddFactAsync(expectedFact, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(default));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.mockEngine.Verify(
            e => e.AddFactAsync(expectedFact, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenEngineReturnsFailure_PropagatesFailure()
    {
        // Arrange
        string fact = "(invalid)";
        this.mockEngine
            .Setup(e => e.AddFactAsync(fact, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Failure("Invalid fact format"));

        // Act
        var result = await this.tool.InvokeAsync(fact);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid fact format");
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_PassesTokenToEngine()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        string fact = "(color sky blue)";
        this.mockEngine
            .Setup(e => e.AddFactAsync(fact, cts.Token))
            .ReturnsAsync(Result<Unit, string>.Success(default));

        // Act
        var result = await this.tool.InvokeAsync(fact, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.mockEngine.Verify(
            e => e.AddFactAsync(fact, cts.Token),
            Times.Once);
    }

    #endregion
}
