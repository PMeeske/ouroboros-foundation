namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Deep tests for AdvancedToolBuilder covering Switch predicate ordering,
/// Aggregate with custom aggregators, and Pipeline composition edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class AdvancedToolBuilderDeepTests
{
    #region Pipeline - Advanced Composition

    [Fact]
    public async Task Pipeline_EmptyToolsList_ReturnsInputUnchanged()
    {
        var pipeline = AdvancedToolBuilder.Pipeline("p", "Pipeline");

        var result = await pipeline.InvokeAsync("passthrough");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("passthrough");
    }

    [Fact]
    public async Task Pipeline_ThreeStepTransformation_ProducesCorrectResult()
    {
        var step1 = new DelegateTool("s1", "S1", (string s) => s.Trim());
        var step2 = new DelegateTool("s2", "S2", (string s) => s.ToUpper());
        var step3 = new DelegateTool("s3", "S3", (string s) => $"<<{s}>>");

        var pipeline = AdvancedToolBuilder.Pipeline("p", "P", step1, step2, step3);
        var result = await pipeline.InvokeAsync("  hello  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("<<HELLO>>");
    }

    [Fact]
    public async Task Pipeline_IntermediateFailure_StopsExecution()
    {
        int lastExecuted = 0;
        var s1 = new DelegateTool("s1", "S1", (string s) => { lastExecuted = 1; return s; });
        var s2 = new DelegateTool("s2", "S2", (string s, CancellationToken ct) =>
        {
            lastExecuted = 2;
            return Task.FromResult(Result<string, string>.Failure("stop"));
        });
        var s3 = new DelegateTool("s3", "S3", (string s) => { lastExecuted = 3; return s; });

        var pipeline = AdvancedToolBuilder.Pipeline("p", "P", s1, s2, s3);
        var result = await pipeline.InvokeAsync("input");

        result.IsFailure.Should().BeTrue();
        lastExecuted.Should().Be(2);
    }

    #endregion

    #region Switch - Predicate Priority

    [Fact]
    public async Task Switch_FirstMatchWins_WhenMultiplePredicatesMatch()
    {
        var t1 = new DelegateTool("t1", "T1", (string s) => "first");
        var t2 = new DelegateTool("t2", "T2", (string s) => "second");

        var switchTool = AdvancedToolBuilder.Switch("sw", "SW",
            (_ => true, t1),   // always matches
            (_ => true, t2));  // also matches

        var result = await switchTool.InvokeAsync("any");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("first");
    }

    [Fact]
    public async Task Switch_ToolFails_PropagatesFailure()
    {
        var failTool = new DelegateTool("f", "F", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("tool-fail")));

        var switchTool = AdvancedToolBuilder.Switch("sw", "SW",
            (_ => true, failTool));

        var result = await switchTool.InvokeAsync("input");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("tool-fail");
    }

    [Fact]
    public async Task Switch_MultipleCases_SelectsCorrectByContent()
    {
        var numTool = new DelegateTool("n", "N", (string s) => $"num:{s}");
        var txtTool = new DelegateTool("t", "T", (string s) => $"txt:{s}");

        var switchTool = AdvancedToolBuilder.Switch("sw", "SW",
            (s => s.All(char.IsDigit), numTool),
            (s => s.All(char.IsLetter), txtTool));

        var numResult = await switchTool.InvokeAsync("123");
        var txtResult = await switchTool.InvokeAsync("abc");

        numResult.Value.Should().Be("num:123");
        txtResult.Value.Should().Be("txt:abc");
    }

    #endregion

    #region Aggregate - Custom Aggregators

    [Fact]
    public async Task Aggregate_CountAggregator_ReturnsCount()
    {
        var t1 = new DelegateTool("t1", "T1", (string s) => "a");
        var t2 = new DelegateTool("t2", "T2", (string s) => "b");
        var t3 = new DelegateTool("t3", "T3", (string s) => "c");

        var agg = AdvancedToolBuilder.Aggregate("agg", "Agg",
            results => $"count={results.Count}",
            t1, t2, t3);

        var result = await agg.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("count=3");
    }

    [Fact]
    public async Task Aggregate_JsonAggregator_CombinesResults()
    {
        var t1 = new DelegateTool("t1", "T1", (string s) => "result-a");
        var t2 = new DelegateTool("t2", "T2", (string s) => "result-b");

        var agg = AdvancedToolBuilder.Aggregate("agg", "Agg",
            results => $"[{string.Join(",", results.Select(r => $"\"{r}\""))}]",
            t1, t2);

        var result = await agg.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("result-a");
        result.Value.Should().Contain("result-b");
    }

    [Fact]
    public async Task Aggregate_MixedSuccessAndFailure_OnlyAggregatesSuccesses()
    {
        var ok1 = new DelegateTool("ok1", "OK1", (string s) => "good-1");
        var fail = new DelegateTool("fail", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("err")));
        var ok2 = new DelegateTool("ok2", "OK2", (string s) => "good-2");

        var agg = AdvancedToolBuilder.Aggregate("agg", "Agg",
            results => string.Join("+", results),
            ok1, fail, ok2);

        var result = await agg.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("good-1+good-2");
    }

    [Fact]
    public async Task Aggregate_EmptyToolsList_ReturnsFailure()
    {
        var agg = AdvancedToolBuilder.Aggregate("agg", "Agg",
            results => string.Join(",", results));

        var result = await agg.InvokeAsync("input");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Aggregate_AggregatorReturnsEmpty_StillSucceeds()
    {
        var t = new DelegateTool("t", "T", (string s) => "data");

        var agg = AdvancedToolBuilder.Aggregate("agg", "Agg",
            _ => "",
            t);

        var result = await agg.InvokeAsync("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region Properties

    [Fact]
    public void Pipeline_SetsNameAndDescription()
    {
        var p = AdvancedToolBuilder.Pipeline("my-pipe", "Pipe desc");

        p.Name.Should().Be("my-pipe");
        p.Description.Should().Be("Pipe desc");
    }

    [Fact]
    public void Switch_SetsNameAndDescription()
    {
        var s = AdvancedToolBuilder.Switch("my-switch", "Switch desc");

        s.Name.Should().Be("my-switch");
        s.Description.Should().Be("Switch desc");
    }

    [Fact]
    public void Aggregate_SetsNameAndDescription()
    {
        var a = AdvancedToolBuilder.Aggregate("my-agg", "Agg desc", r => "");

        a.Name.Should().Be("my-agg");
        a.Description.Should().Be("Agg desc");
    }

    #endregion
}
