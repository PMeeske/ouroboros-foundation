using Ouroboros.Core.Steps;
using Ouroboros.Abstractions.Monads;

namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class ContextualStepDefinitionTests
{
    [Fact]
    public void Constructor_WithNullContextStep_Throws()
    {
        Step<object, Step<int, int>> step = null!;
        var act = () => new ContextualStepDefinition<int, int, object>(step);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullContextualStep_Throws()
    {
        ContextualStep<int, int, object> step = null!;
        var act = () => new ContextualStepDefinition<int, int, object>(step);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Constructor_WithSyncFunc_WrapsCorrectly()
    {
        var def = new ContextualStepDefinition<int, string, object>(x => x.ToString());
        var (result, logs) = await def.InvokeAsync(42, new object());

        result.Should().Be("42");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task Constructor_WithSyncFuncAndLog_EmitsLog()
    {
        var def = new ContextualStepDefinition<int, string, object>(x => x.ToString(), "log1");
        var (result, logs) = await def.InvokeAsync(42, new object());

        result.Should().Be("42");
        logs.Should().ContainSingle("log1");
    }

    [Fact]
    public async Task Constructor_WithPureAsyncStep_WrapsCorrectly()
    {
        Step<int, string> step = x => Task.FromResult(x.ToString());
        var def = new ContextualStepDefinition<int, string, object>(step);

        var (result, logs) = await def.InvokeAsync(7, new object());
        result.Should().Be("7");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task Constructor_WithPureAsyncStepAndLog_EmitsLog()
    {
        Step<int, string> step = x => Task.FromResult(x.ToString());
        var def = new ContextualStepDefinition<int, string, object>(step, "logged");

        var (result, logs) = await def.InvokeAsync(7, new object());
        result.Should().Be("7");
        logs.Should().ContainSingle("logged");
    }

    [Fact]
    public async Task Constructor_WithContextStep_ResolvesContext()
    {
        Step<string, Step<int, string>> ctxStep = ctx =>
            Task.FromResult<Step<int, string>>(x => Task.FromResult($"{ctx}:{x}"));

        var def = new ContextualStepDefinition<int, string, string>(ctxStep);
        var (result, _) = await def.InvokeAsync(5, "ctx");

        result.Should().Be("ctx:5");
    }

    [Fact]
    public async Task LiftPure_CreatesDefinition()
    {
        var def = ContextualStepDefinition<int, string, object>.LiftPure(x => x.ToString());
        var (result, _) = await def.InvokeAsync(42, new object());
        result.Should().Be("42");
    }

    [Fact]
    public async Task FromPure_CreatesDefinition()
    {
        Step<int, string> step = x => Task.FromResult(x.ToString());
        var def = ContextualStepDefinition<int, string, object>.FromPure(step);

        var (result, _) = await def.InvokeAsync(42, new object());
        result.Should().Be("42");
    }

    [Fact]
    public async Task FromContext_CreatesDefinition()
    {
        Step<string, Step<int, string>> ctxStep = ctx =>
            Task.FromResult<Step<int, string>>(x => Task.FromResult($"{ctx}:{x}"));

        var def = ContextualStepDefinition<int, string, string>.FromContext(ctxStep);
        var (result, _) = await def.InvokeAsync(5, "abc");
        result.Should().Be("abc:5");
    }

    [Fact]
    public async Task Pipe_ContextualStep_ComposesCorrectly()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        ContextualStep<int, string, object> next = (input, _) =>
            Task.FromResult((input.ToString(), new List<string> { "next-log" }));

        var composed = def.Pipe(next);
        var (result, logs) = await composed.InvokeAsync(9, new object());

        result.Should().Be("10");
        logs.Should().Contain("next-log");
    }

    [Fact]
    public async Task Pipe_PureAsyncStep_ComposesCorrectly()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        Step<int, string> pure = x => Task.FromResult(x.ToString());

        var composed = def.Pipe(pure);
        var (result, _) = await composed.InvokeAsync(9, new object());

        result.Should().Be("10");
    }

    [Fact]
    public async Task Pipe_PureAsyncStepWithLog_EmitsLog()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        Step<int, string> pure = x => Task.FromResult(x.ToString());

        var composed = def.Pipe(pure, "piped");
        var (_, logs) = await composed.InvokeAsync(9, new object());

        logs.Should().Contain("piped");
    }

    [Fact]
    public async Task Pipe_SyncFunc_ComposesCorrectly()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        var composed = def.Pipe<string>(x => x.ToString());

        var (result, _) = await composed.InvokeAsync(9, new object());
        result.Should().Be("10");
    }

    [Fact]
    public async Task Pipe_SyncFuncWithLog_EmitsLog()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        var composed = def.Pipe<string>(x => x.ToString(), "func-log");

        var (_, logs) = await composed.InvokeAsync(9, new object());
        logs.Should().Contain("func-log");
    }

    [Fact]
    public async Task ImplicitConversion_ToContextualStep_Works()
    {
        var def = ContextualStepDefinition<int, string, object>.LiftPure(x => x.ToString());
        ContextualStep<int, string, object> step = def;

        var (result, _) = await step(42, new object());
        result.Should().Be("42");
    }

    [Fact]
    public void Invoke_ExecutesSynchronously()
    {
        var def = ContextualStepDefinition<int, string, object>.LiftPure(x => x.ToString());
        var (result, logs) = def.Invoke(42, new object());

        result.Should().Be("42");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task Forget_BindsContextAndRetainsLogs()
    {
        var def = ContextualStepDefinition<int, string, object>.LiftPure(x => x.ToString(), "log1");
        Step<int, (string result, List<string> logs)> step = def.Forget(new object());

        var (result, logs) = await step(42);
        result.Should().Be("42");
        logs.Should().ContainSingle("log1");
    }

    [Fact]
    public async Task ForgetAll_BindsContextAndDiscardsLogs()
    {
        var def = ContextualStepDefinition<int, string, object>.LiftPure(x => x.ToString(), "log1");
        Step<int, string> step = def.ForgetAll(new object());

        var result = await step(42);
        result.Should().Be("42");
    }

    [Fact]
    public async Task WithLog_AppendsStaticLogMessage()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        var withLog = def.WithLog("step-executed");

        var (result, logs) = await withLog.InvokeAsync(9, new object());
        result.Should().Be(10);
        logs.Should().Contain("step-executed");
    }

    [Fact]
    public async Task WithConditionalLog_WhenFunctionReturnsString_AppendsLog()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        var withLog = def.WithConditionalLog(x => x > 5 ? $"big: {x}" : null);

        var (result, logs) = await withLog.InvokeAsync(9, new object());
        result.Should().Be(10);
        logs.Should().Contain("big: 10");
    }

    [Fact]
    public async Task WithConditionalLog_WhenFunctionReturnsNull_NoLog()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        var withLog = def.WithConditionalLog(x => x > 100 ? "big" : null);

        var (_, logs) = await withLog.InvokeAsync(9, new object());
        logs.Should().NotContain("big");
    }

    [Fact]
    public async Task TryStep_Success_ReturnsSuccessResult()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        var tryDef = def.TryStep();

        var (result, _) = await tryDef.InvokeAsync(9, new object());
        result.IsSuccess.Should().BeTrue();
        result.Match(v => v, _ => -1).Should().Be(10);
    }

    [Fact]
    public async Task TryStep_Exception_ReturnsFailureResult()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(
            new Func<int, int>(_ => throw new InvalidOperationException("boom")));
        var tryDef = def.TryStep();

        var (result, _) = await tryDef.InvokeAsync(9, new object());
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TryOption_PredicateTrue_ReturnsSome()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        var optionDef = def.TryOption(x => x > 0);

        var (result, _) = await optionDef.InvokeAsync(9, new object());
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task TryOption_PredicateFalse_ReturnsNone()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        var optionDef = def.TryOption(x => x > 100);

        var (result, _) = await optionDef.InvokeAsync(9, new object());
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task MultiplePipes_ChainCorrectly()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1)
            .Pipe<int>(x => x * 2)
            .Pipe<string>(x => $"answer={x}");

        var (result, _) = await def.InvokeAsync(4, new object());
        result.Should().Be("answer=10");
    }
}
