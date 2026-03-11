namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class ContextualStepExtensionsAdditionalTests
{
    [Fact]
    public async Task TryStep_OnOperationCanceledException_Rethrows()
    {
        ContextualStep<int, string, object> step = (input, ctx) =>
            throw new OperationCanceledException();
        var safe = step.TryStep();

        var act = async () => await safe(42, new object());
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryOption_OnOperationCanceledException_Rethrows()
    {
        ContextualStep<int, int, object> step = (input, ctx) =>
            throw new OperationCanceledException();
        var optional = step.TryOption<int, int, object>(_ => true);

        var act = async () => await optional(42, new object());
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Then_AccumulatesLogsFromMultipleSteps()
    {
        var step1 = ContextualStep.LiftPure<int, int, object>(x => x + 1, "step1");
        var step2 = ContextualStep.LiftPure<int, int, object>(x => x * 2, "step2");
        var step3 = ContextualStep.LiftPure<int, string, object>(x => x.ToString(), "step3");

        var composed = step1.Then(step2).Then(step3);
        var (result, logs) = await composed(4, new object());

        result.Should().Be("10"); // (4+1)*2 = 10
        logs.Should().HaveCount(3);
        logs.Should().ContainInOrder("step1", "step2", "step3");
    }

    [Fact]
    public async Task Map_WithLog_AppendsToExistingLogs()
    {
        var step = ContextualStep.LiftPure<int, int, object>(x => x + 1, "original");
        var mapped = step.Map<int, int, string, object>(x => x.ToString(), "mapped");

        var (result, logs) = await mapped(5, new object());
        result.Should().Be("6");
        logs.Should().HaveCount(2);
        logs[0].Should().Be("original");
        logs[1].Should().Be("mapped");
    }

    [Fact]
    public async Task WithLog_AppendsToExistingLogs()
    {
        var step = ContextualStep.LiftPure<int, int, object>(x => x, "original");
        var logged = step.WithLog<int, int, object>("additional");

        var (_, logs) = await logged(1, new object());
        logs.Should().HaveCount(2);
        logs[0].Should().Be("original");
        logs[1].Should().Be("additional");
    }

    [Fact]
    public async Task TryStep_OnSuccess_PreservesLogs()
    {
        var step = ContextualStep.LiftPure<int, string, object>(x => x.ToString(), "logged");
        var safe = step.TryStep();

        var (result, logs) = await safe(42, new object());
        result.IsSuccess.Should().BeTrue();
        logs.Should().ContainSingle("logged");
    }

    [Fact]
    public async Task TryOption_WhenPredicateTrue_PreservesLogs()
    {
        var step = ContextualStep.LiftPure<int, int, object>(x => x * 2, "logged");
        var optional = step.TryOption<int, int, object>(x => x > 0);

        var (result, logs) = await optional(5, new object());
        result.HasValue.Should().BeTrue();
        logs.Should().ContainSingle("logged");
    }

    [Fact]
    public async Task Forget_WithContext_ExecutesWithCorrectContext()
    {
        ContextualStep<int, string, string> step = (input, ctx) =>
            Task.FromResult(($"{ctx}:{input}", new List<string> { "log" }));

        var pure = step.Forget("myCtx");
        var (result, logs) = await pure(42);

        result.Should().Be("myCtx:42");
        logs.Should().ContainSingle("log");
    }

    [Fact]
    public async Task ForgetAll_WithContext_DiscardsLogsAndReturnsResult()
    {
        ContextualStep<int, string, string> step = (input, ctx) =>
            Task.FromResult(($"{ctx}:{input}", new List<string> { "log1", "log2" }));

        var pure = step.ForgetAll("ctx");
        var result = await pure(42);

        result.Should().Be("ctx:42");
    }

    [Fact]
    public async Task TryStep_ExceptionMessage_IncludedInLogs()
    {
        ContextualStep<int, string, object> step = (input, ctx) =>
            throw new InvalidOperationException("detailed error");
        var safe = step.TryStep();

        var (result, logs) = await safe(42, new object());
        result.IsSuccess.Should().BeFalse();
        logs.Should().ContainSingle();
        logs[0].Should().Contain("detailed error");
    }

    [Fact]
    public async Task TryOption_ExceptionMessage_IncludedInLogs()
    {
        ContextualStep<int, int, object> step = (input, ctx) =>
            throw new InvalidOperationException("detailed error");
        var optional = step.TryOption<int, int, object>(_ => true);

        var (result, logs) = await optional(42, new object());
        result.HasValue.Should().BeFalse();
        logs.Should().ContainSingle();
        logs[0].Should().Contain("detailed error");
    }
}
