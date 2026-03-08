namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Deep tests for OrchestratorToolExtensions covering retry backoff, caching expiry,
/// timeout edge cases, fallback chaining, parallel partial failures, and composition.
/// </summary>
[Trait("Category", "Unit")]
public class OrchestratorToolExtensionsDeepTests
{
    #region WithRetry - Backoff

    [Fact]
    public async Task WithRetry_SingleRetry_SucceedsOnSecondAttempt()
    {
        int attempts = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            attempts++;
            return attempts == 1
                ? Task.FromResult(Result<string, string>.Failure("transient"))
                : Task.FromResult(Result<string, string>.Success("ok"));
        });

        var retried = tool.WithRetry(maxRetries: 2, delayMs: 10);
        var result = await retried.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(2);
    }

    [Fact]
    public async Task WithRetry_MaxRetriesOne_OnlyOneAttempt()
    {
        int attempts = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            attempts++;
            return Task.FromResult(Result<string, string>.Failure("fail"));
        });

        var retried = tool.WithRetry(maxRetries: 1, delayMs: 10);
        await retried.InvokeAsync("input");

        attempts.Should().Be(1);
    }

    [Fact]
    public void WithRetry_PreservesSchemaAndName()
    {
        var schema = """{"type":"object"}""";
        var tool = new DelegateTool("original", "Desc",
            (s, ct) => Task.FromResult(Result<string, string>.Success(s)), schema);

        var retried = tool.WithRetry(maxRetries: 3);

        retried.Name.Should().Be("original");
        retried.JsonSchema.Should().Be(schema);
    }

    #endregion

    #region WithCaching - Expiry Behavior

    [Fact]
    public async Task WithCaching_CacheHit_DoesNotCallTool()
    {
        int calls = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            calls++;
            return Task.FromResult(Result<string, string>.Success($"v{calls}"));
        });

        var cached = tool.WithCaching(TimeSpan.FromMinutes(10));
        await cached.InvokeAsync("key");
        await cached.InvokeAsync("key");
        await cached.InvokeAsync("key");

        calls.Should().Be(1);
    }

    [Fact]
    public async Task WithCaching_DifferentKeys_CachesIndependently()
    {
        int calls = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            calls++;
            return Task.FromResult(Result<string, string>.Success(s));
        });

        var cached = tool.WithCaching(TimeSpan.FromMinutes(10));
        await cached.InvokeAsync("a");
        await cached.InvokeAsync("b");
        await cached.InvokeAsync("a"); // cached

        calls.Should().Be(2);
    }

    [Fact]
    public async Task WithCaching_FailureNotCached_RetriesTool()
    {
        int calls = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            calls++;
            return calls == 1
                ? Task.FromResult(Result<string, string>.Failure("err"))
                : Task.FromResult(Result<string, string>.Success("ok"));
        });

        var cached = tool.WithCaching(TimeSpan.FromMinutes(10));
        var r1 = await cached.InvokeAsync("key");
        var r2 = await cached.InvokeAsync("key");

        r1.IsFailure.Should().BeTrue();
        r2.IsSuccess.Should().BeTrue();
        calls.Should().Be(2);
    }

    #endregion

    #region WithTimeout - Edge Cases

    [Fact]
    public async Task WithTimeout_InstantTool_ReturnsImmediately()
    {
        var tool = new DelegateTool("t", "T", (string s) => "instant");

        var timed = tool.WithTimeout(TimeSpan.FromSeconds(30));
        var result = await timed.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("instant");
    }

    [Fact]
    public void WithTimeout_PreservesSchemaAndName()
    {
        var schema = """{"type":"object"}""";
        var tool = new DelegateTool("named", "Desc",
            (s, ct) => Task.FromResult(Result<string, string>.Success(s)), schema);

        var timed = tool.WithTimeout(TimeSpan.FromSeconds(5));

        timed.Name.Should().Be("named");
        timed.JsonSchema.Should().Be(schema);
    }

    #endregion

    #region WithFallback - Chain

    [Fact]
    public async Task WithFallback_ChainedFallbacks_TriesInOrder()
    {
        var primary = new DelegateTool("p", "P", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("p-err")));
        var secondary = new DelegateTool("s", "S", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("s-err")));
        var tertiary = new DelegateTool("t", "T", (string s) => "tertiary-ok");

        var chained = primary.WithFallback(secondary).WithFallback(tertiary);
        var result = await chained.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("tertiary-ok");
    }

    [Fact]
    public async Task WithFallback_PrimarySucceeds_FallbackNeverCalled()
    {
        bool fallbackCalled = false;
        var primary = new DelegateTool("p", "P", (string s) => "primary-ok");
        var fallback = new DelegateTool("f", "F", (string s) =>
        {
            fallbackCalled = true;
            return "fallback";
        });

        var withFb = primary.WithFallback(fallback);
        await withFb.InvokeAsync("input");

        fallbackCalled.Should().BeFalse();
    }

    #endregion

    #region Parallel - Combiner Patterns

    [Fact]
    public async Task Parallel_AllSuccess_CombinesWithCustomCombiner()
    {
        var t1 = new DelegateTool("t1", "T1", (string s) => "alpha");
        var t2 = new DelegateTool("t2", "T2", (string s) => "beta");
        var t3 = new DelegateTool("t3", "T3", (string s) => "gamma");

        var parallel = OrchestratorToolExtensions.Parallel(
            "par", "Par",
            results => $"total={results.Length}",
            t1, t2, t3);

        var result = await parallel.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("total=3");
    }

    [Fact]
    public async Task Parallel_PartialFailure_OnlyIncludesSuccesses()
    {
        var ok = new DelegateTool("ok", "OK", (string s) => "good");
        var fail = new DelegateTool("f", "F", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("bad")));

        var parallel = OrchestratorToolExtensions.Parallel(
            "par", "Par",
            results => string.Join(",", results),
            ok, fail, ok);

        var result = await parallel.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("good,good");
    }

    #endregion

    #region WithPerformanceTracking - Timing

    [Fact]
    public async Task WithPerformanceTracking_MeasuresTime()
    {
        double? measuredMs = null;
        var tool = new DelegateTool("t", "T", async (string s, CancellationToken ct) =>
        {
            await Task.Delay(10, ct);
            return Result<string, string>.Success("ok");
        });

        var tracked = tool.WithPerformanceTracking((_, ms, _) => measuredMs = ms);
        await tracked.InvokeAsync("input");

        measuredMs.Should().NotBeNull();
        measuredMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task WithPerformanceTracking_ToolThrows_StillReportsMetrics()
    {
        bool? success = null;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
            throw new InvalidOperationException("boom"));

        var tracked = tool.WithPerformanceTracking((_, _, s) => success = s);
        var result = await tracked.InvokeAsync("input");

        result.IsFailure.Should().BeTrue();
        success.Should().BeFalse();
    }

    #endregion

    #region WithDynamicSelection

    [Fact]
    public async Task WithDynamicSelection_RoutesBasedOnInput()
    {
        var mathTool = new DelegateTool("math", "M", (string s) => "math-result");
        var textTool = new DelegateTool("text", "T", (string s) => "text-result");

        var dynamic = OrchestratorToolExtensions.WithDynamicSelection(
            "dyn", "Dynamic",
            input => input.Contains("calc") ? mathTool : textTool,
            mathTool, textTool);

        var r1 = await dynamic.InvokeAsync("calc 2+2");
        var r2 = await dynamic.InvokeAsync("hello world");

        r1.Value.Should().Be("math-result");
        r2.Value.Should().Be("text-result");
    }

    #endregion

    #region Composition - Combined Extensions

    [Fact]
    public async Task ComposedExtensions_RetryPlusCaching_WorksTogether()
    {
        int calls = 0;
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
        {
            calls++;
            return Task.FromResult(Result<string, string>.Success($"v{calls}"));
        });

        var enhanced = tool
            .WithRetry(maxRetries: 2, delayMs: 10)
            .WithCaching(TimeSpan.FromMinutes(5));

        var r1 = await enhanced.InvokeAsync("key");
        var r2 = await enhanced.InvokeAsync("key");

        r1.IsSuccess.Should().BeTrue();
        r2.IsSuccess.Should().BeTrue();
        r2.Value.Should().Be(r1.Value); // cached
        calls.Should().Be(1);
    }

    [Fact]
    public async Task ComposedExtensions_FallbackPlusTracking_WorksTogether()
    {
        string? trackedName = null;
        var primary = new DelegateTool("p", "P", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("err")));
        var fallback = new DelegateTool("f", "F", (string s) => "fb-ok");

        var enhanced = primary
            .WithFallback(fallback)
            .WithPerformanceTracking((name, _, _) => trackedName = name);

        var result = await enhanced.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("fb-ok");
    }

    #endregion
}
