namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Deep tests for ToolBuilder covering chain edge cases, FirstSuccess ordering,
/// and Conditional error handling paths.
/// </summary>
[Trait("Category", "Unit")]
public class ToolBuilderDeepTests
{
    #region Chain - Empty Tools Array

    [Fact]
    public async Task Chain_NoTools_ReturnsOriginalInput()
    {
        var chain = ToolBuilder.Chain("empty", "Empty chain");

        var result = await chain.InvokeAsync("passthrough");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("passthrough");
    }

    #endregion

    #region Chain - Multiple Sequential Failures

    [Fact]
    public async Task Chain_FirstToolFails_SecondNeverExecuted()
    {
        bool secondExecuted = false;
        var fail = new DelegateTool("f", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("err")));
        var second = new DelegateTool("s", "Second", (string s) =>
        {
            secondExecuted = true;
            return s;
        });

        var chain = ToolBuilder.Chain("c", "Chain", fail, second);
        var result = await chain.InvokeAsync("input");

        result.IsFailure.Should().BeTrue();
        secondExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task Chain_MiddleToolFails_LastNeverExecuted()
    {
        int executionCount = 0;
        var first = new DelegateTool("t1", "T1", (string s) =>
        {
            executionCount++;
            return $"step1:{s}";
        });
        var failing = new DelegateTool("t2", "T2", (string s, CancellationToken ct) =>
        {
            executionCount++;
            return Task.FromResult(Result<string, string>.Failure("mid-fail"));
        });
        var last = new DelegateTool("t3", "T3", (string s) =>
        {
            executionCount++;
            return $"step3:{s}";
        });

        var chain = ToolBuilder.Chain("c", "C", first, failing, last);
        var result = await chain.InvokeAsync("input");

        result.IsFailure.Should().BeTrue();
        executionCount.Should().Be(2);
    }

    #endregion

    #region Chain - Data Transformation Pipeline

    [Fact]
    public async Task Chain_ThreeToolTransformation_ProducesCorrectOutput()
    {
        var trim = new DelegateTool("trim", "Trim", (string s) => s.Trim());
        var upper = new DelegateTool("upper", "Upper", (string s) => s.ToUpper());
        var wrap = new DelegateTool("wrap", "Wrap", (string s) => $"[{s}]");

        var chain = ToolBuilder.Chain("pipeline", "Pipeline", trim, upper, wrap);
        var result = await chain.InvokeAsync("  hello  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("[HELLO]");
    }

    #endregion

    #region Chain - Cancellation in Middle

    [Fact]
    public async Task Chain_CancelledBeforeSecond_ReturnsFailure()
    {
        using var cts = new CancellationTokenSource();
        var first = new DelegateTool("t1", "T1", (string s, CancellationToken ct) =>
        {
            cts.Cancel();
            return Task.FromResult(Result<string, string>.Success(s));
        });
        var second = new DelegateTool("t2", "T2", (string s) => s.ToUpper());

        var chain = ToolBuilder.Chain("c", "C", first, second);
        var result = await chain.InvokeAsync("input", cts.Token);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region FirstSuccess - Ordering

    [Fact]
    public async Task FirstSuccess_StopsAtFirstMatch()
    {
        int secondCallCount = 0;
        var success = new DelegateTool("ok", "OK", (string s) => "first-ok");
        var second = new DelegateTool("s", "S", (string s) =>
        {
            secondCallCount++;
            return "second";
        });

        var fs = ToolBuilder.FirstSuccess("fs", "FS", success, second);
        await fs.InvokeAsync("input");

        secondCallCount.Should().Be(0);
    }

    [Fact]
    public async Task FirstSuccess_EmptyToolsList_ReturnsAllToolsFailed()
    {
        var fs = ToolBuilder.FirstSuccess("fs", "FS");

        var result = await fs.InvokeAsync("input");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("All tools failed");
    }

    #endregion

    #region Conditional - Various Selector Behaviors

    [Fact]
    public async Task Conditional_SelectorReturnsBasedOnContent_Works()
    {
        var numTool = new DelegateTool("num", "Num", (string s) => $"number:{s}");
        var textTool = new DelegateTool("txt", "Txt", (string s) => $"text:{s}");

        var cond = ToolBuilder.Conditional("c", "C",
            input => int.TryParse(input, out _) ? numTool : textTool);

        var numResult = await cond.InvokeAsync("42");
        var textResult = await cond.InvokeAsync("hello");

        numResult.Value.Should().Be("number:42");
        textResult.Value.Should().Be("text:hello");
    }

    [Fact]
    public async Task Conditional_SelectedToolFails_PropagatesFailure()
    {
        var failTool = new DelegateTool("f", "F", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("selected-fail")));

        var cond = ToolBuilder.Conditional("c", "C", _ => failTool);
        var result = await cond.InvokeAsync("input");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("selected-fail");
    }

    [Fact]
    public async Task Conditional_SelectorThrowsNonInvalidOp_PropagatesException()
    {
        var cond = ToolBuilder.Conditional("c", "C",
            _ => throw new ArgumentException("not InvalidOp"));

        // ArgumentException is not caught by Conditional (only InvalidOperationException is)
        Func<Task> act = () => cond.InvokeAsync("input");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Properties

    [Fact]
    public void Chain_SetsNameAndDescription()
    {
        var chain = ToolBuilder.Chain("my-chain", "My chain desc");

        chain.Name.Should().Be("my-chain");
        chain.Description.Should().Be("My chain desc");
    }

    [Fact]
    public void FirstSuccess_SetsNameAndDescription()
    {
        var fs = ToolBuilder.FirstSuccess("my-fs", "FS desc");

        fs.Name.Should().Be("my-fs");
        fs.Description.Should().Be("FS desc");
    }

    [Fact]
    public void Conditional_SetsNameAndDescription()
    {
        var cond = ToolBuilder.Conditional("my-cond", "Cond desc",
            _ => new DelegateTool("t", "T", (string s) => s));

        cond.Name.Should().Be("my-cond");
        cond.Description.Should().Be("Cond desc");
    }

    #endregion
}
