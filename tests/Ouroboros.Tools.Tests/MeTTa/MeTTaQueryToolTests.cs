// <copyright file="MeTTaQueryToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.MeTTa;

using FluentAssertions;
using Moq;
using Ouroboros.Tools;
using Ouroboros.Tools.MeTTa;
using Xunit;

/// <summary>
/// Unit tests for <see cref="MeTTaQueryTool"/>.
/// </summary>
[Trait("Category", "Unit")]
public class MeTTaQueryToolTests
{
    private readonly Mock<IMeTTaEngine> mockEngine = new();
    private readonly MeTTaQueryTool tool;

    public MeTTaQueryToolTests()
    {
        this.tool = new MeTTaQueryTool(this.mockEngine.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullEngine_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new MeTTaQueryTool(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("engine");
    }

    [Fact]
    public void Constructor_WithValidEngine_CreatesInstance()
    {
        // Act
        var sut = new MeTTaQueryTool(this.mockEngine.Object);

        // Assert
        sut.Should().NotBeNull();
    }

    #endregion

    #region ITool Interface Tests

    [Fact]
    public void Name_ReturnsExpectedValue()
    {
        // Assert
        this.tool.Name.Should().Be("metta_query");
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
    public void JsonSchema_ContainsQueryProperty()
    {
        // Assert
        this.tool.JsonSchema.Should().Contain("query");
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
    public async Task InvokeAsync_WithDirectQuery_DelegatesToEngine()
    {
        // Arrange
        string query = "!(+ 2 3)";
        this.mockEngine
            .Setup(e => e.ExecuteQueryAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("5"));

        // Act
        var result = await this.tool.InvokeAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("5");
        this.mockEngine.Verify(
            e => e.ExecuteQueryAsync(query, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithJsonInput_ExtractsQueryFromJson()
    {
        // Arrange
        string jsonInput = """{"query": "!(match &self (fact $x) $x)"}""";
        string expectedQuery = "!(match &self (fact $x) $x)";
        this.mockEngine
            .Setup(e => e.ExecuteQueryAsync(expectedQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("result"));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.mockEngine.Verify(
            e => e.ExecuteQueryAsync(expectedQuery, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithJsonMissingQueryProp_UsesRawInput()
    {
        // Arrange
        string jsonInput = """{"other": "value"}""";
        this.mockEngine
            .Setup(e => e.ExecuteQueryAsync(jsonInput, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("ok"));

        // Act
        var result = await this.tool.InvokeAsync(jsonInput);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.mockEngine.Verify(
            e => e.ExecuteQueryAsync(jsonInput, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenEngineReturnsFailure_PropagatesFailure()
    {
        // Arrange
        string query = "!(invalid)";
        this.mockEngine
            .Setup(e => e.ExecuteQueryAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Failure("Syntax error"));

        // Act
        var result = await this.tool.InvokeAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Syntax error");
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_PassesTokenToEngine()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        string query = "!(+ 1 1)";
        this.mockEngine
            .Setup(e => e.ExecuteQueryAsync(query, cts.Token))
            .ReturnsAsync(Result<string, string>.Success("2"));

        // Act
        var result = await this.tool.InvokeAsync(query, cts.Token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        this.mockEngine.Verify(
            e => e.ExecuteQueryAsync(query, cts.Token),
            Times.Once);
    }

    #endregion
}
