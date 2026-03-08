// <copyright file="MonadicToolExtensionsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Tests for MonadicToolExtensions covering ToStep, ToKleisli, Then, OrElse, Map, ToContextual.
/// </summary>
[Trait("Category", "Unit")]
public class MonadicToolExtensionsTests
{
    #region ToStep Tests

    [Fact]
    public async Task ToStep_SuccessfulTool_ReturnsSuccessResult()
    {
        // Arrange
        var tool = new DelegateTool("upper", "Upper", (string s) => s.ToUpper());
        var step = tool.ToStep();

        // Act
        var result = await step("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("HELLO");
    }

    [Fact]
    public async Task ToStep_FailingTool_ReturnsFailureResult()
    {
        // Arrange
        var tool = new DelegateTool("fail", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("tool error")));
        var step = tool.ToStep();

        // Act
        var result = await step("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("tool error");
    }

    #endregion

    #region ToKleisli Tests

    [Fact]
    public async Task ToKleisli_SuccessfulTool_ReturnsSuccessResult()
    {
        // Arrange
        var tool = new DelegateTool("t", "T", (string s) => $"processed:{s}");
        var kleisli = tool.ToKleisli();

        // Act
        var result = await kleisli("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("processed:input");
    }

    #endregion

    #region Then Tests

    [Fact]
    public async Task Then_BothToolsSucceed_ChainsResults()
    {
        // Arrange
        var first = new DelegateTool("first", "First", (string s) => $"first:{s}");
        var second = new DelegateTool("second", "Second", (string s) => $"second:{s}");
        var chained = first.Then(second);

        // Act
        var result = await chained("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("second:first:input");
    }

    [Fact]
    public async Task Then_FirstToolFails_SecondToolNotInvoked()
    {
        // Arrange
        var first = new DelegateTool("fail", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("first failed")));
        bool secondInvoked = false;
        var second = new DelegateTool("second", "Second", (string s) =>
        {
            secondInvoked = true;
            return s;
        });
        var chained = first.Then(second);

        // Act
        var result = await chained("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("first failed");
        secondInvoked.Should().BeFalse();
    }

    #endregion

    #region OrElse Tests

    [Fact]
    public async Task OrElse_FirstToolSucceeds_FallbackNotInvoked()
    {
        // Arrange
        var first = new DelegateTool("first", "First", (string s) => $"first:{s}");
        bool fallbackInvoked = false;
        var fallback = new DelegateTool("fallback", "Fallback", (string s) =>
        {
            fallbackInvoked = true;
            return $"fallback:{s}";
        });
        var step = first.OrElse(fallback);

        // Act
        var result = await step("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("first:input");
        fallbackInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task OrElse_FirstToolFails_InvokesFallback()
    {
        // Arrange
        var first = new DelegateTool("fail", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("first failed")));
        var fallback = new DelegateTool("fallback", "Fallback", (string s) => $"fallback:{s}");
        var step = first.OrElse(fallback);

        // Act
        var result = await step("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("fallback:input");
    }

    [Fact]
    public async Task OrElse_BothToolsFail_ReturnsFallbackFailure()
    {
        // Arrange
        var first = new DelegateTool("f1", "F1", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("first error")));
        var fallback = new DelegateTool("f2", "F2", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("fallback error")));
        var step = first.OrElse(fallback);

        // Act
        var result = await step("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("fallback error");
    }

    #endregion

    #region Map Tests

    [Fact]
    public async Task Map_SuccessfulTool_MapsResult()
    {
        // Arrange
        var tool = new DelegateTool("t", "T", (string s) => "42");
        var mapped = tool.Map(int.Parse);

        // Act
        var result = await mapped("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Map_FailingTool_PropagatesError()
    {
        // Arrange
        var tool = new DelegateTool("fail", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("error")));
        var mapped = tool.Map(int.Parse);

        // Act
        var result = await mapped("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("error");
    }

    [Fact]
    public async Task Map_TransformToStringLength_Works()
    {
        // Arrange
        var tool = new DelegateTool("t", "T", (string s) => s.ToUpper());
        var mapped = tool.Map(s => s.Length);

        // Act
        var result = await mapped("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    #endregion

    #region ToContextual Tests

    [Fact]
    public async Task ToContextual_WithDefaultLogMessage_ReturnsToolNameLog()
    {
        // Arrange
        var tool = new DelegateTool("my-tool", "My Tool", (string s) => $"out:{s}");
        var contextual = tool.ToContextual<string>();

        // Act
        var (result, logs) = await contextual("input", "context");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("out:input");
        logs.Should().Contain("Tool 'my-tool' executed");
    }

    [Fact]
    public async Task ToContextual_WithCustomLogMessage_UsesCustomMessage()
    {
        // Arrange
        var tool = new DelegateTool("t", "T", (string s) => s);
        var contextual = tool.ToContextual<string>("Custom log message");

        // Act
        var (result, logs) = await contextual("input", "context");

        // Assert
        logs.Should().Contain("Custom log message");
    }

    [Fact]
    public async Task ToContextual_WithFailingTool_StillReturnsLog()
    {
        // Arrange
        var tool = new DelegateTool("fail", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("error")));
        var contextual = tool.ToContextual<int>();

        // Act
        var (result, logs) = await contextual("input", 42);

        // Assert
        result.IsFailure.Should().BeTrue();
        logs.Should().NotBeEmpty();
    }

    #endregion
}
