using Ouroboros.Abstractions;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public class ContextualStepExtensionsTests
{
    private static ContextualStep<int, string, object> CreateIntToStringStep() =>
        ContextualStep.LiftPure<int, string, object>(x => x.ToString(), "first");

    private static ContextualStep<string, int, object> CreateStringToIntStep() =>
        ContextualStep.LiftPure<string, int, object>(s => s.Length, "second");

    [Fact]
    public async Task Then_ComposesStepsSequentially()
    {
        var first = CreateIntToStringStep();
        var second = CreateStringToIntStep();

        var composed = first.Then(second);
        var (result, logs) = await composed(42, new object());

        result.Should().Be(2); // "42".Length
        logs.Should().HaveCount(2);
        logs[0].Should().Be("first");
        logs[1].Should().Be("second");
    }

    [Fact]
    public async Task Map_TransformsOutput()
    {
        var step = CreateIntToStringStep();
        var mapped = step.Map<int, string, int, object>(s => s.Length, "mapped");

        var (result, logs) = await mapped(42, new object());
        result.Should().Be(2); // "42".Length
        logs.Should().Contain("first");
        logs.Should().Contain("mapped");
    }

    [Fact]
    public async Task Map_WithoutLog_DoesNotAddLog()
    {
        var step = ContextualStep.LiftPure<int, string, object>(x => x.ToString());
        var mapped = step.Map<int, string, int, object>(s => s.Length);

        var (result, logs) = await mapped(42, new object());
        result.Should().Be(2);
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task WithLog_AppendsLogMessage()
    {
        var step = ContextualStep.LiftPure<int, int, object>(x => x * 2);
        var logged = step.WithLog<int, int, object>("doubled");

        var (result, logs) = await logged(5, new object());
        result.Should().Be(10);
        logs.Should().ContainSingle("doubled");
    }

    [Fact]
    public async Task WithConditionalLog_AddsLogWhenNonNull()
    {
        var step = ContextualStep.LiftPure<int, int, object>(x => x * 2);
        var logged = step.WithConditionalLog<int, int, object>(
            result => result > 5 ? "big result" : null);

        var (result, logs) = await logged(5, new object());
        result.Should().Be(10);
        logs.Should().ContainSingle("big result");
    }

    [Fact]
    public async Task WithConditionalLog_SkipsLogWhenNull()
    {
        var step = ContextualStep.LiftPure<int, int, object>(x => x * 2);
        var logged = step.WithConditionalLog<int, int, object>(
            result => result > 100 ? "big result" : null);

        var (result, logs) = await logged(5, new object());
        result.Should().Be(10);
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task Forget_BindsContextAndReturnsTupleStep()
    {
        var step = ContextualStep.LiftPure<int, string, string>(x => x.ToString(), "log");
        Step<int, (string result, List<string> logs)> pure = step.Forget("context");

        var (result, logs) = await pure(42);
        result.Should().Be("42");
        logs.Should().ContainSingle("log");
    }

    [Fact]
    public async Task ForgetAll_BindsContextAndDiscardsLogs()
    {
        var step = ContextualStep.LiftPure<int, string, string>(x => x.ToString(), "log");
        Step<int, string> pure = step.ForgetAll("context");

        var result = await pure(42);
        result.Should().Be("42");
    }

    [Fact]
    public async Task TryStep_OnSuccess_ReturnsSuccessResult()
    {
        var step = ContextualStep.LiftPure<int, string, object>(x => x.ToString());
        var safe = step.TryStep();

        var (result, logs) = await safe(42, new object());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public async Task TryStep_OnException_ReturnsFailureResult()
    {
        ContextualStep<int, string, object> step = (input, ctx) =>
            throw new InvalidOperationException("test error");
        var safe = step.TryStep();

        var (result, logs) = await safe(42, new object());
        result.IsSuccess.Should().BeFalse();
        logs.Should().ContainSingle(l => l.Contains("test error"));
    }

    [Fact]
    public async Task TryOption_WhenPredicateTrue_ReturnsSome()
    {
        var step = ContextualStep.LiftPure<int, int, object>(x => x * 2);
        var optional = step.TryOption<int, int, object>(x => x > 5);

        var (result, logs) = await optional(5, new object());
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task TryOption_WhenPredicateFalse_ReturnsNone()
    {
        var step = ContextualStep.LiftPure<int, int, object>(x => x * 2);
        var optional = step.TryOption<int, int, object>(x => x > 100);

        var (result, logs) = await optional(5, new object());
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task TryOption_OnException_ReturnsNone()
    {
        ContextualStep<int, int, object> step = (input, ctx) =>
            throw new InvalidOperationException("test error");
        var optional = step.TryOption<int, int, object>(_ => true);

        var (result, logs) = await optional(42, new object());
        result.HasValue.Should().BeFalse();
        logs.Should().ContainSingle(l => l.Contains("test error"));
    }
}
