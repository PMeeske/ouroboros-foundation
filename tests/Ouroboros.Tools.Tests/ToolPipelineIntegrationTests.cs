namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Integration-style unit tests verifying tool composition patterns work
/// end-to-end within a ToolRegistry context.
/// </summary>
[Trait("Category", "Unit")]
public class ToolPipelineIntegrationTests
{
    #region Registry + Builder Composition

    [Fact]
    public async Task Registry_WithBuiltTools_WorksEndToEnd()
    {
        var trim = new DelegateTool("trim", "Trim", (string s) => s.Trim());
        var upper = new DelegateTool("upper", "Upper", (string s) => s.ToUpper());
        var chain = ToolBuilder.Chain("process", "Process text", trim, upper);

        var registry = new ToolRegistry().WithTool(chain);

        var tool = registry.Get("process");
        tool.Should().NotBeNull();

        var result = await tool!.InvokeAsync("  hello  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("HELLO");
    }

    [Fact]
    public async Task Registry_WithFirstSuccess_SelectsWorkingTool()
    {
        var failing = new DelegateTool("f", "F", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("unavailable")));
        var working = new DelegateTool("w", "W", (string s) => $"worked:{s}");

        var fallback = ToolBuilder.FirstSuccess("resilient", "Resilient tool",
            failing, working);

        var registry = new ToolRegistry().WithTool(fallback);
        var tool = registry.Get("resilient");
        var result = await tool!.InvokeAsync("data");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("worked:data");
    }

    #endregion

    #region Registry + Advanced Builder

    [Fact]
    public async Task Registry_WithSwitch_RoutesCorrectly()
    {
        var numTool = new DelegateTool("n", "N", (string s) => "number");
        var textTool = new DelegateTool("t", "T", (string s) => "text");

        var switchTool = AdvancedToolBuilder.Switch("router", "Routes input",
            (s => s.All(char.IsDigit), numTool),
            (_ => true, textTool));

        var registry = new ToolRegistry().WithTool(switchTool);

        var tool = registry.Get("router");
        var numResult = await tool!.InvokeAsync("123");
        var textResult = await tool.InvokeAsync("hello");

        numResult.Value.Should().Be("number");
        textResult.Value.Should().Be("text");
    }

    [Fact]
    public async Task Registry_WithAggregate_CombinesResults()
    {
        var t1 = new DelegateTool("t1", "T1", (string s) => "A");
        var t2 = new DelegateTool("t2", "T2", (string s) => "B");

        var agg = AdvancedToolBuilder.Aggregate("combined", "Combined",
            results => string.Join("-", results), t1, t2);

        var registry = new ToolRegistry().WithTool(agg);

        var result = await registry.Get("combined")!.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("A");
        result.Value.Should().Contain("B");
    }

    #endregion

    #region Registry + Orchestrator Extensions

    [Fact]
    public async Task Registry_WithTrackedTool_ReportsMetrics()
    {
        double? ms = null;
        var tool = new DelegateTool("fast", "Fast", (string s) => "quick");
        var tracked = tool.WithPerformanceTracking((_, elapsed, _) => ms = elapsed);

        var registry = new ToolRegistry().WithTool(tracked);
        await registry.Get("fast")!.InvokeAsync("input");

        ms.Should().NotBeNull();
    }

    [Fact]
    public async Task Registry_WithCachedTool_CachesResult()
    {
        int calls = 0;
        var tool = new DelegateTool("cached", "Cached", (string s, CancellationToken ct) =>
        {
            calls++;
            return Task.FromResult(Result<string, string>.Success($"v{calls}"));
        });
        var cached = tool.WithCaching(TimeSpan.FromMinutes(5));

        var registry = new ToolRegistry().WithTool(cached);
        var t = registry.Get("cached")!;

        await t.InvokeAsync("key");
        await t.InvokeAsync("key");

        calls.Should().Be(1);
    }

    [Fact]
    public async Task Registry_WithRetryTool_RetriesOnFailure()
    {
        int attempts = 0;
        var tool = new DelegateTool("retry", "Retry", (string s, CancellationToken ct) =>
        {
            attempts++;
            return attempts < 3
                ? Task.FromResult(Result<string, string>.Failure("transient"))
                : Task.FromResult(Result<string, string>.Success("ok"));
        });
        var retried = tool.WithRetry(maxRetries: 3, delayMs: 10);

        var registry = new ToolRegistry().WithTool(retried);
        var result = await registry.Get("retry")!.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        attempts.Should().Be(3);
    }

    #endregion

    #region Registry + Monadic Extensions

    [Fact]
    public async Task MonadicComposition_ThenAndOrElse_Combined()
    {
        var primary = new DelegateTool("p", "P", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("p-err")));
        var fallback = new DelegateTool("f", "F", (string s) => $"fb:{s}");
        var transform = new DelegateTool("t", "T", (string s) => s.ToUpper());

        // OrElse: primary fails, fallback succeeds
        var step1 = primary.OrElse(fallback);
        var result1 = await step1("input");

        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be("fb:input");

        // Then: take result and transform
        var step2 = fallback.Then(transform);
        var result2 = await step2("input");

        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Be("FB:INPUT");
    }

    [Fact]
    public async Task MonadicComposition_MapAfterThen()
    {
        var first = new DelegateTool("f", "F", (string s) => "42");
        var mapped = first.Map(int.Parse);

        var result = await mapped("ignored");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    #endregion

    #region CreateDefault Registry

    [Fact]
    public async Task CreateDefault_MathTool_EvaluatesComplexExpression()
    {
        var registry = ToolRegistry.CreateDefault();
        var math = registry.Get("math")!;

        var result = await math.InvokeAsync("2+3*4");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("14");
    }

    [Fact]
    public void CreateDefault_ExportSchemas_ReturnsValidJson()
    {
        var registry = ToolRegistry.CreateDefault();

        var schemas = registry.ExportSchemas();

        schemas.Should().NotBeNullOrEmpty();
        schemas.Should().Contain("math");
    }

    #endregion
}
