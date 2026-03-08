namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Deep tests for MonadicToolExtensions covering chaining, composition patterns,
/// Map transformations, ToContextual with different context types.
/// </summary>
[Trait("Category", "Unit")]
public class MonadicToolExtensionsDeepTests
{
    #region Then - Multiple Chaining

    [Fact]
    public async Task Then_ThreeTools_ChainsInOrder()
    {
        var t1 = new DelegateTool("t1", "T1", (string s) => $"a:{s}");
        var t2 = new DelegateTool("t2", "T2", (string s) => $"b:{s}");
        var t3 = new DelegateTool("t3", "T3", (string s) => $"c:{s}");

        var step = t1.Then(t2);
        // Execute first two, then execute third
        var result1 = await step("input");
        var finalInput = result1.Match(s => s, _ => "");
        var result2 = await t3.InvokeAsync(finalInput);

        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Be("c:b:a:input");
    }

    [Fact]
    public async Task Then_SecondToolFails_PropagatesFailure()
    {
        var first = new DelegateTool("t1", "T1", (string s) => $"first:{s}");
        var second = new DelegateTool("t2", "T2", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("second-fail")));

        var step = first.Then(second);
        var result = await step("input");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("second-fail");
    }

    #endregion

    #region OrElse - Fallback Chains

    [Fact]
    public async Task OrElse_ThreeTools_FindsFirstSuccess()
    {
        var fail1 = new DelegateTool("f1", "F1", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("err1")));
        var fail2 = new DelegateTool("f2", "F2", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("err2")));
        var success = new DelegateTool("ok", "OK", (string s) => "found-it");

        // Chain OrElse: fail1 || fail2 || success
        var step1 = fail1.OrElse(fail2);
        var result1 = await step1("input");

        result1.IsFailure.Should().BeTrue();

        // Then try success
        var step2 = fail1.OrElse(success);
        var result2 = await step2("input");

        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Be("found-it");
    }

    [Fact]
    public async Task OrElse_FirstSucceeds_DoesNotExecuteSecond()
    {
        bool secondCalled = false;
        var first = new DelegateTool("t1", "T1", (string s) => "first-ok");
        var second = new DelegateTool("t2", "T2", (string s) =>
        {
            secondCalled = true;
            return "second";
        });

        var step = first.OrElse(second);
        await step("input");

        secondCalled.Should().BeFalse();
    }

    #endregion

    #region Map - Various Transformations

    [Fact]
    public async Task Map_ToBool_TransformsCorrectly()
    {
        var tool = new DelegateTool("t", "T", (string s) => "true");
        var mapped = tool.Map(bool.Parse);

        var result = await mapped("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Map_ToCustomType_TransformsCorrectly()
    {
        var tool = new DelegateTool("t", "T", (string s) => "42");
        var mapped = tool.Map(s => new { Number = int.Parse(s) });

        var result = await mapped("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Number.Should().Be(42);
    }

    [Fact]
    public async Task Map_OnFailure_DoesNotTransform()
    {
        var tool = new DelegateTool("t", "T", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("err")));
        bool mapperCalled = false;
        var mapped = tool.Map(s =>
        {
            mapperCalled = true;
            return s.Length;
        });

        var result = await mapped("input");

        result.IsFailure.Should().BeTrue();
        mapperCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Map_ToStringArray_TransformsCorrectly()
    {
        var tool = new DelegateTool("t", "T", (string s) => "a,b,c");
        var mapped = tool.Map(s => s.Split(','));

        var result = await mapped("input");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    #endregion

    #region ToStep - Round Trip

    [Fact]
    public async Task ToStep_FromDelegateTool_ExecutesCorrectly()
    {
        var tool = new DelegateTool("process", "Process",
            (string s) => $"processed:{s}");
        var step = tool.ToStep();

        var result = await step("data");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("processed:data");
    }

    [Fact]
    public async Task ToStep_EmptyInput_Works()
    {
        var tool = new DelegateTool("t", "T", (string s) => $"len={s.Length}");
        var step = tool.ToStep();

        var result = await step("");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("len=0");
    }

    #endregion

    #region ToKleisli - Composition

    [Fact]
    public async Task ToKleisli_MultipleInvocations_Independent()
    {
        var tool = new DelegateTool("t", "T", (string s) => $"out:{s}");
        var kleisli = tool.ToKleisli();

        var r1 = await kleisli("a");
        var r2 = await kleisli("b");

        r1.Value.Should().Be("out:a");
        r2.Value.Should().Be("out:b");
    }

    #endregion

    #region ToContextual - Different Context Types

    [Fact]
    public async Task ToContextual_IntContext_Works()
    {
        var tool = new DelegateTool("t", "T", (string s) => s);
        var contextual = tool.ToContextual<int>();

        var (result, logs) = await contextual("input", 42);

        result.IsSuccess.Should().BeTrue();
        logs.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ToContextual_ComplexContext_Works()
    {
        var tool = new DelegateTool("t", "T", (string s) => s);
        var contextual = tool.ToContextual<Dictionary<string, object>>();

        var ctx = new Dictionary<string, object> { ["key"] = "value" };
        var (result, logs) = await contextual("input", ctx);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ToContextual_NullLogMessage_UsesDefault()
    {
        var tool = new DelegateTool("my-tool", "Desc", (string s) => s);
        var contextual = tool.ToContextual<string>(logMessage: null);

        var (_, logs) = await contextual("input", "ctx");

        logs.Should().Contain(l => l.Contains("my-tool"));
    }

    #endregion
}
