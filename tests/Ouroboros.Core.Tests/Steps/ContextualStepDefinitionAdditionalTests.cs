namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class ContextualStepDefinitionAdditionalTests
{
    [Fact]
    public async Task TryOption_OnException_ReturnsNone()
    {
        var def = new ContextualStepDefinition<int, int, object>(
            new Func<int, int>(_ => throw new InvalidOperationException("boom")));
        var optionDef = def.TryOption(_ => true);

        var (result, logs) = await optionDef.InvokeAsync(9, new object());
        result.HasValue.Should().BeFalse();
        logs.Should().ContainSingle(l => l.Contains("boom"));
    }

    [Fact]
    public async Task TryStep_OnOperationCanceledException_Rethrows()
    {
        var def = new ContextualStepDefinition<int, int, object>(
            new Func<int, int>(_ => throw new OperationCanceledException()));
        var tryDef = def.TryStep();

        var act = async () => await tryDef.InvokeAsync(9, new object());
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Invoke_WithLog_ReturnsLogsCorrectly()
    {
        var def = ContextualStepDefinition<int, string, object>.LiftPure(
            x => x.ToString(), "log-entry");
        var (result, logs) = def.Invoke(42, new object());

        result.Should().Be("42");
        logs.Should().ContainSingle("log-entry");
    }

    [Fact]
    public async Task Pipe_ContextualStep_ChainsMultiple()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1);
        ContextualStep<int, int, object> step2 =
            ContextualStep.LiftPure<int, int, object>(x => x * 2, "doubled");
        ContextualStep<int, string, object> step3 =
            ContextualStep.LiftPure<int, string, object>(x => x.ToString(), "stringified");

        var composed = def.Pipe(step2).Pipe(step3);
        var (result, logs) = await composed.InvokeAsync(4, new object());

        result.Should().Be("10"); // (4+1)*2 = 10
        logs.Should().Contain("doubled");
        logs.Should().Contain("stringified");
    }

    [Fact]
    public async Task Forget_ChainedPipeline_RetainsAllLogs()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1, "step1")
            .Pipe<int>(x => x * 2, "step2");

        var step = def.Forget(new object());
        var (result, logs) = await step(4);

        result.Should().Be(10); // (4+1)*2 = 10
        logs.Should().Contain("step1");
        logs.Should().Contain("step2");
    }

    [Fact]
    public async Task ForgetAll_ChainedPipeline_DiscardsAllLogs()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x + 1, "step1")
            .Pipe<int>(x => x * 2, "step2");

        var step = def.ForgetAll(new object());
        var result = await step(4);

        result.Should().Be(10);
    }

    [Fact]
    public async Task WithLog_Multiple_AccumulatesAll()
    {
        var def = ContextualStepDefinition<int, int, object>.LiftPure(x => x)
            .WithLog("log1")
            .WithLog("log2")
            .WithLog("log3");

        var (_, logs) = await def.InvokeAsync(1, new object());
        logs.Should().HaveCount(3);
        logs.Should().ContainInOrder("log1", "log2", "log3");
    }

    [Fact]
    public async Task LiftPure_WithLog_StaticFactory_Works()
    {
        var def = ContextualStepDefinition<int, string, object>.LiftPure(
            x => x.ToString(), "created");
        var (result, logs) = await def.InvokeAsync(42, new object());

        result.Should().Be("42");
        logs.Should().ContainSingle("created");
    }

    [Fact]
    public async Task FromPure_WithLog_StaticFactory_Works()
    {
        Step<int, string> step = x => Task.FromResult(x.ToString());
        var def = ContextualStepDefinition<int, string, object>.FromPure(step, "from-pure");

        var (result, logs) = await def.InvokeAsync(7, new object());
        result.Should().Be("7");
        logs.Should().ContainSingle("from-pure");
    }
}
