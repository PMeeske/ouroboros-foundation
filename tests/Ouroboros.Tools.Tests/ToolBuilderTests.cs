// <copyright file="ToolBuilderTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Tests for ToolBuilder which provides monadic tool composition patterns.
/// </summary>
[Trait("Category", "Unit")]
public class ToolBuilderTests
{
    // --- Chain ---

    [Fact]
    public async Task Chain_SingleTool_InvokesSuccessfully()
    {
        // Arrange
        var tool = new DelegateTool("upper", "Uppercase", (string s) => s.ToUpper());
        var chain = ToolBuilder.Chain("chained", "Chained tool", tool);

        // Act
        var result = await chain.InvokeAsync("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("HELLO");
    }

    [Fact]
    public async Task Chain_MultipleTools_PipesThroughAll()
    {
        // Arrange
        var addPrefix = new DelegateTool("prefix", "Add prefix", (string s) => $"prefix-{s}");
        var addSuffix = new DelegateTool("suffix", "Add suffix", (string s) => $"{s}-suffix");
        var chain = ToolBuilder.Chain("chained", "Chained tool", addPrefix, addSuffix);

        // Act
        var result = await chain.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("prefix-input-suffix");
    }

    [Fact]
    public async Task Chain_ToolFails_StopsChainEarly()
    {
        // Arrange
        var failingTool = new DelegateTool("fail", "Fails", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("tool failed")));
        var neverReached = new DelegateTool("after", "After fail",
            (string s) => $"should not reach: {s}");

        var chain = ToolBuilder.Chain("chained", "Chained", failingTool, neverReached);

        // Act
        var result = await chain.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("tool failed");
    }

    [Fact]
    public async Task Chain_Cancellation_ReturnsFailure()
    {
        // Arrange
        var slowTool = new DelegateTool("slow", "Slow tool", async (string s, CancellationToken ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Result<string, string>.Success(s);
        });

        var chain = ToolBuilder.Chain("chained", "Chained", slowTool);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await chain.InvokeAsync("input", cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Chain_Properties_SetCorrectly()
    {
        var chain = ToolBuilder.Chain("my-chain", "My chain description",
            new DelegateTool("t1", "Tool 1", (string s) => s));

        chain.Name.Should().Be("my-chain");
        chain.Description.Should().Be("My chain description");
    }

    // --- FirstSuccess ---

    [Fact]
    public async Task FirstSuccess_FirstToolSucceeds_ReturnsItsResult()
    {
        // Arrange
        var tool1 = new DelegateTool("t1", "Tool 1", (string s) => $"t1:{s}");
        var tool2 = new DelegateTool("t2", "Tool 2", (string s) => $"t2:{s}");
        var firstSuccess = ToolBuilder.FirstSuccess("first", "First success", tool1, tool2);

        // Act
        var result = await firstSuccess.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("t1:input");
    }

    [Fact]
    public async Task FirstSuccess_FirstToolFails_TriesSecond()
    {
        // Arrange
        var failTool = new DelegateTool("fail", "Fails", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("failed")));
        var successTool = new DelegateTool("ok", "Succeeds", (string s) => $"ok:{s}");
        var firstSuccess = ToolBuilder.FirstSuccess("first", "First success", failTool, successTool);

        // Act
        var result = await firstSuccess.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok:input");
    }

    [Fact]
    public async Task FirstSuccess_AllToolsFail_ReturnsAllToolsFailedError()
    {
        // Arrange
        var fail1 = new DelegateTool("f1", "Fail 1", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("fail1")));
        var fail2 = new DelegateTool("f2", "Fail 2", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("fail2")));
        var firstSuccess = ToolBuilder.FirstSuccess("first", "First success", fail1, fail2);

        // Act
        var result = await firstSuccess.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("All tools failed");
    }

    [Fact]
    public async Task FirstSuccess_Cancellation_ReturnsFailure()
    {
        var tool = new DelegateTool("t", "Tool", async (string s, CancellationToken ct) =>
        {
            ct.ThrowIfCancellationRequested();
            return Result<string, string>.Success(s);
        });

        var firstSuccess = ToolBuilder.FirstSuccess("first", "First", tool);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await firstSuccess.InvokeAsync("input", cts.Token);
        result.IsFailure.Should().BeTrue();
    }

    // --- Conditional ---

    [Fact]
    public async Task Conditional_SelectsCorrectTool()
    {
        // Arrange
        var upperTool = new DelegateTool("upper", "Upper", (string s) => s.ToUpper());
        var lowerTool = new DelegateTool("lower", "Lower", (string s) => s.ToLower());

        var conditional = ToolBuilder.Conditional("cond", "Conditional",
            input => input.StartsWith("up") ? upperTool : lowerTool);

        // Act
        var result1 = await conditional.InvokeAsync("upper this");
        var result2 = await conditional.InvokeAsync("lower THIS");

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be("UPPER THIS");

        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Be("lower this");
    }

    [Fact]
    public async Task Conditional_SelectorThrows_ReturnsFailure()
    {
        // Arrange
        var conditional = ToolBuilder.Conditional("cond", "Conditional",
            input => throw new InvalidOperationException("bad selector"));

        // Act
        var result = await conditional.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Tool selection failed");
    }
}
