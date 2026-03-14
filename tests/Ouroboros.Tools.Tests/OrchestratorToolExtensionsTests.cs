// <copyright file="OrchestratorToolExtensionsTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Tests for OrchestratorToolExtensions covering performance tracking, retry,
/// caching, timeout, fallback, parallel execution, and dynamic selection.
/// </summary>
[Trait("Category", "Unit")]
public class OrchestratorToolExtensionsTests
{
    #region WithPerformanceTracking Tests

    [Fact]
    public async Task WithPerformanceTracking_SuccessfulTool_ReportsSuccessMetrics()
    {
        // Arrange
        var tool = new DelegateTool("t", "T", (string s) => s.ToUpper());
        string? reportedName = null;
        double? reportedMs = null;
        bool? reportedSuccess = null;

        var tracked = tool.WithPerformanceTracking((name, ms, success) =>
        {
            reportedName = name;
            reportedMs = ms;
            reportedSuccess = success;
        });

        // Act
        var result = await tracked.InvokeAsync("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("HELLO");
        reportedName.Should().Be("t");
        reportedMs.Should().BeGreaterThanOrEqualTo(0);
        reportedSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task WithPerformanceTracking_FailingTool_ReportsFailureMetrics()
    {
        // Arrange
        var tool = new DelegateTool("fail", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("error")));
        bool? reportedSuccess = null;

        var tracked = tool.WithPerformanceTracking((_, _, success) => reportedSuccess = success);

        // Act
        var result = await tracked.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        reportedSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task WithPerformanceTracking_ThrowingTool_ReportsFailureMetrics()
    {
        // Arrange
        var tool = new DelegateTool("throw", "Throw", (string s, CancellationToken ct) =>
            throw new InvalidOperationException("boom"));
        bool? reportedSuccess = null;

        var tracked = tool.WithPerformanceTracking((_, _, success) => reportedSuccess = success);

        // Act
        var result = await tracked.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        reportedSuccess.Should().BeFalse();
    }

    [Fact]
    public void WithPerformanceTracking_PreservesSchema()
    {
        // Arrange
        var schema = """{"type":"object"}""";
        var tool = new DelegateTool("t", "T",
            (s, ct) => Task.FromResult(Result<string, string>.Success(s)), schema);

        // Act
        var tracked = tool.WithPerformanceTracking((_, _, _) => { });

        // Assert
        tracked.JsonSchema.Should().Be(schema);
    }

    #endregion

    #region WithDynamicSelection Tests

    [Fact]
    public async Task WithDynamicSelection_SelectsCorrectTool()
    {
        // Arrange
        var math = new DelegateTool("math", "Math", (string s) => "math-result");
        var text = new DelegateTool("text", "Text", (string s) => "text-result");

        var dynamic = OrchestratorToolExtensions.WithDynamicSelection(
            "dynamic", "Dynamic",
            input => input.Contains("calc") ? math : text,
            math, text);

        // Act
        var result = await dynamic.InvokeAsync("calc 2+2");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("math-result");
    }

    [Fact]
    public async Task WithDynamicSelection_SelectorReturnsNull_ReturnsFailure()
    {
        // Arrange
        var dynamic = OrchestratorToolExtensions.WithDynamicSelection(
            "dynamic", "Dynamic",
            _ => null!);

        // Act
        var result = await dynamic.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No suitable tool");
    }

    [Fact]
    public async Task WithDynamicSelection_SelectorThrows_ReturnsFailure()
    {
        // Arrange
        var dynamic = OrchestratorToolExtensions.WithDynamicSelection(
            "dynamic", "Dynamic",
            _ => throw new InvalidOperationException("selector error"));

        // Act
        var result = await dynamic.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Tool selection failed");
    }

    #endregion

    #region Parallel Tests

    [Fact]
    public async Task Parallel_AllToolsSucceed_CombinesResults()
    {
        // Arrange
        var t1 = new DelegateTool("t1", "T1", (string s) => "a");
        var t2 = new DelegateTool("t2", "T2", (string s) => "b");

        var parallel = OrchestratorToolExtensions.Parallel(
            "par", "Parallel",
            results => string.Join(",", results),
            t1, t2);

        // Act
        var result = await parallel.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("a");
        result.Value.Should().Contain("b");
    }

    [Fact]
    public async Task Parallel_AllToolsFail_ReturnsFailure()
    {
        // Arrange
        var f1 = new DelegateTool("f1", "F1", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("error1")));
        var f2 = new DelegateTool("f2", "F2", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("error2")));

        var parallel = OrchestratorToolExtensions.Parallel(
            "par", "Parallel",
            results => string.Join(",", results),
            f1, f2);

        // Act
        var result = await parallel.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("All parallel tool executions failed");
    }

    [Fact]
    public async Task Parallel_SomeToolsFail_IncludesOnlySuccesses()
    {
        // Arrange
        var ok = new DelegateTool("ok", "OK", (string s) => "success");
        var fail = new DelegateTool("fail", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("error")));

        var parallel = OrchestratorToolExtensions.Parallel(
            "par", "Parallel",
            results => string.Join(",", results),
            ok, fail);

        // Act
        var result = await parallel.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("success");
    }

    #endregion

    #region WithRetry Tests

    [Fact]
    public async Task WithRetry_SucceedsFirstTime_ReturnsImmediately()
    {
        // Arrange
        int invocationCount = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            invocationCount++;
            return Task.FromResult(Result<string, string>.Success("ok"));
        });

        var retried = tool.WithRetry(maxRetries: 3, delayMs: 10);

        // Act
        var result = await retried.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        invocationCount.Should().Be(1);
    }

    [Fact]
    public async Task WithRetry_FailsThenSucceeds_RetriesAndSucceeds()
    {
        // Arrange
        int invocationCount = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            invocationCount++;
            return invocationCount < 3
                ? Task.FromResult(Result<string, string>.Failure("retry"))
                : Task.FromResult(Result<string, string>.Success("ok"));
        });

        var retried = tool.WithRetry(maxRetries: 3, delayMs: 10);

        // Act
        var result = await retried.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        invocationCount.Should().Be(3);
    }

    [Fact]
    public async Task WithRetry_AllRetriesFail_ReturnsLastFailure()
    {
        // Arrange
        var tool = new DelegateTool("fail", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("persistent error")));

        var retried = tool.WithRetry(maxRetries: 2, delayMs: 10);

        // Act
        var result = await retried.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("persistent error");
    }

    [Fact]
    public async Task WithRetry_Cancelled_ReturnsFailure()
    {
        // Arrange
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("will retry")));

        var retried = tool.WithRetry(maxRetries: 5, delayMs: 100);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await retried.InvokeAsync("input", cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region WithCaching Tests

    [Fact]
    public async Task WithCaching_SameInput_ReturnsCachedResult()
    {
        // Arrange
        int invocationCount = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            invocationCount++;
            return Task.FromResult(Result<string, string>.Success($"result-{invocationCount}"));
        });

        var cached = tool.WithCaching(TimeSpan.FromMinutes(5));

        // Act
        var result1 = await cached.InvokeAsync("same-input");
        var result2 = await cached.InvokeAsync("same-input");

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be("result-1");
        result2.Value.Should().Be("result-1"); // Cached!
        invocationCount.Should().Be(1);
    }

    [Fact]
    public async Task WithCaching_DifferentInput_DoesNotUseCachedResult()
    {
        // Arrange
        int invocationCount = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            invocationCount++;
            return Task.FromResult(Result<string, string>.Success($"result-{s}"));
        });

        var cached = tool.WithCaching(TimeSpan.FromMinutes(5));

        // Act
        await cached.InvokeAsync("input-a");
        await cached.InvokeAsync("input-b");

        // Assert
        invocationCount.Should().Be(2);
    }

    [Fact]
    public async Task WithCaching_FailedResult_DoesNotCache()
    {
        // Arrange
        int invocationCount = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            invocationCount++;
            return invocationCount == 1
                ? Task.FromResult(Result<string, string>.Failure("first error"))
                : Task.FromResult(Result<string, string>.Success("retry ok"));
        });

        var cached = tool.WithCaching(TimeSpan.FromMinutes(5));

        // Act
        var result1 = await cached.InvokeAsync("input");
        var result2 = await cached.InvokeAsync("input");

        // Assert
        result1.IsFailure.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        invocationCount.Should().Be(2);
    }

    #endregion

    #region WithTimeout Tests

    [Fact]
    public async Task WithTimeout_FastTool_ReturnsResult()
    {
        // Arrange
        var tool = new DelegateTool("fast", "Fast", (string s) => "quick");
        var timed = tool.WithTimeout(TimeSpan.FromSeconds(5));

        // Act
        var result = await timed.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("quick");
    }

    [Fact]
    public async Task WithTimeout_SlowTool_ReturnsTimeoutFailure()
    {
        // Arrange
        var tool = new DelegateTool("slow", "Slow", async (string s, CancellationToken ct) =>
        {
            await Task.Delay(5000, ct);
            return Result<string, string>.Success("too late");
        });

        var timed = tool.WithTimeout(TimeSpan.FromMilliseconds(50));

        // Act
        var result = await timed.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("timed out");
    }

    #endregion

    #region WithFallback Tests

    [Fact]
    public async Task WithFallback_PrimarySucceeds_ReturnsPrimaryResult()
    {
        // Arrange
        var primary = new DelegateTool("primary", "Primary", (string s) => "primary-result");
        var fallback = new DelegateTool("fallback", "Fallback", (string s) => "fallback-result");
        var withFallback = primary.WithFallback(fallback);

        // Act
        var result = await withFallback.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("primary-result");
    }

    [Fact]
    public async Task WithFallback_PrimaryFails_ReturnsFallbackResult()
    {
        // Arrange
        var primary = new DelegateTool("primary", "Primary", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("primary failed")));
        var fallback = new DelegateTool("fallback", "Fallback", (string s) => "fallback-result");
        var withFallback = primary.WithFallback(fallback);

        // Act
        var result = await withFallback.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("fallback-result");
    }

    [Fact]
    public async Task WithFallback_BothFail_ReturnsFallbackFailure()
    {
        // Arrange
        var primary = new DelegateTool("p", "P", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("primary error")));
        var fallback = new DelegateTool("f", "F", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("fallback error")));
        var withFallback = primary.WithFallback(fallback);

        // Act
        var result = await withFallback.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("fallback error");
    }

    [Fact]
    public void WithFallback_PreservesSchema()
    {
        // Arrange
        var schema = """{"type":"object"}""";
        var primary = new DelegateTool("p", "P",
            (s, ct) => Task.FromResult(Result<string, string>.Success(s)), schema);
        var fallback = new DelegateTool("f", "F", (string s) => s);

        // Act
        var withFallback = primary.WithFallback(fallback);

        // Assert
        withFallback.JsonSchema.Should().Be(schema);
    }

    #endregion
}
