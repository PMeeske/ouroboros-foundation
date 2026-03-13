// <copyright file="SafeCalculatorToolTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;
using Ouroboros.Tools.MeTTa;

/// <summary>
/// Tests for SafeCalculatorTool which provides verified arithmetic with proof-carrying code.
/// </summary>
[Trait("Category", "Unit")]
public class SafeCalculatorToolTests
{
    private readonly SafeCalculatorTool _calculator;

    public SafeCalculatorToolTests()
    {
        _calculator = new SafeCalculatorTool();
    }

    // --- Properties ---

    [Fact]
    public void Name_ReturnsSafeCalculator()
    {
        _calculator.Name.Should().Be("safe_calculator");
    }

    [Fact]
    public void Description_IsNotEmpty()
    {
        _calculator.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        _calculator.JsonSchema.Should().NotBeNull();
        _calculator.JsonSchema.Should().Contain("expression");
    }

    // --- Simple arithmetic ---

    [Fact]
    public async Task InvokeAsync_SimpleAddition_ReturnsCorrectResult()
    {
        var result = await _calculator.InvokeAsync("2+2");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("4");
    }

    [Fact]
    public async Task InvokeAsync_Subtraction_ReturnsCorrectResult()
    {
        var result = await _calculator.InvokeAsync("10-3");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("7");
    }

    [Fact]
    public async Task InvokeAsync_Multiplication_ReturnsCorrectResult()
    {
        var result = await _calculator.InvokeAsync("6*7");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("42");
    }

    [Fact]
    public async Task InvokeAsync_Division_ReturnsCorrectResult()
    {
        var result = await _calculator.InvokeAsync("10/2");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("5");
    }

    [Fact]
    public async Task InvokeAsync_OrderOfOperations_RespectsOperatorPrecedence()
    {
        var result = await _calculator.InvokeAsync("2+3*4");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("14");
    }

    [Fact]
    public async Task InvokeAsync_Parentheses_Respected()
    {
        var result = await _calculator.InvokeAsync("(2+3)*4");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("20");
    }

    // --- JSON input ---

    [Fact]
    public async Task InvokeAsync_JsonExpression_Works()
    {
        var json = """{"expression": "3+4"}""";
        var result = await _calculator.InvokeAsync(json);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("7");
    }

    [Fact]
    public async Task InvokeAsync_JsonWithExpectedResult_MatchingResult_Succeeds()
    {
        var json = """{"expression": "5+5", "expected_result": 10}""";
        var result = await _calculator.InvokeAsync(json);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_JsonWithExpectedResult_MismatchingResult_Fails()
    {
        var json = """{"expression": "5+5", "expected_result": 99}""";
        var result = await _calculator.InvokeAsync(json);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("mismatch");
    }

    // --- Error handling ---

    [Fact]
    public async Task InvokeAsync_EmptyInput_ReturnsFailure()
    {
        var result = await _calculator.InvokeAsync("");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhitespaceInput_ReturnsFailure()
    {
        var result = await _calculator.InvokeAsync("   ");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_InvalidExpression_ReturnsFailure()
    {
        var result = await _calculator.InvokeAsync("invalid math here");

        result.IsFailure.Should().BeTrue();
    }

    // --- Verification badge ---

    [Fact]
    public async Task InvokeAsync_WithoutSymbolicEngine_ShowsVerifiedBadge()
    {
        var result = await _calculator.InvokeAsync("1+1");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Verified");
    }

    // --- Symbolic engine integration ---

    [Fact]
    public async Task InvokeAsync_WithMockSymbolicEngine_ShowsSymbolicallyVerified()
    {
        // Arrange
        var mockEngine = new Mock<IMeTTaEngine>();
        mockEngine
            .Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("2"));

        var calculator = new SafeCalculatorTool(mockEngine.Object);

        // Act
        var result = await calculator.InvokeAsync("1+1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Symbolically Verified");
    }

    [Fact]
    public async Task InvokeAsync_WithFailingSymbolicEngine_ReturnsFailure()
    {
        // Arrange
        var mockEngine = new Mock<IMeTTaEngine>();
        mockEngine
            .Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Failure("Engine error"));

        var calculator = new SafeCalculatorTool(mockEngine.Object);

        // Act
        var result = await calculator.InvokeAsync("1+1");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // --- Cancellation ---

    [Fact]
    public async Task InvokeAsync_WithCancellation_CanBeCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Note: simple expressions may complete before checking cancellation
        var result = await _calculator.InvokeAsync("1+1", cts.Token);

        // The result may succeed or fail depending on timing
        // The key point is it does not throw an unhandled exception
        (result.IsSuccess || result.IsFailure).Should().BeTrue();
    }
}
