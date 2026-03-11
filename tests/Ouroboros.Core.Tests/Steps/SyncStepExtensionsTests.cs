using Ouroboros.Core.Steps;
using Ouroboros.Abstractions.Monads;

namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class SyncStepExtensionsTests
{
    [Fact]
    public void ToSyncStep_LiftsFunction()
    {
        Func<int, string> func = x => x.ToString();
        var step = func.ToSyncStep();

        step.Invoke(42).Should().Be("42");
    }

    [Fact]
    public void ToSync_ConvertsAsyncStepToSync()
    {
        Step<int, int> asyncStep = x => Task.FromResult(x * 2);
        var syncStep = asyncStep.ToSync();

        syncStep.Invoke(5).Should().Be(10);
    }

    [Fact]
    public async Task Then_SyncThenAsync_ComposesProperly()
    {
        var syncStep = new SyncStep<int, int>(x => x + 1);
        Step<int, string> asyncStep = x => Task.FromResult($"result={x}");

        Step<int, string> composed = syncStep.Then(asyncStep);
        var result = await composed(9);

        result.Should().Be("result=10");
    }

    [Fact]
    public async Task Then_AsyncThenSync_ComposesProperly()
    {
        Step<int, int> asyncStep = x => Task.FromResult(x + 1);
        var syncStep = new SyncStep<int, string>(x => $"result={x}");

        Step<int, string> composed = asyncStep.Then(syncStep);
        var result = await composed(9);

        result.Should().Be("result=10");
    }

    [Fact]
    public void TrySync_Success_ReturnsSuccessResult()
    {
        var step = new SyncStep<int, int>(x => x * 2);
        var tryStep = step.TrySync();

        var result = tryStep.Invoke(5);
        result.IsSuccess.Should().BeTrue();
        result.Match(v => v, _ => -1).Should().Be(10);
    }

    [Fact]
    public void TrySync_Exception_ReturnsFailureResult()
    {
        var step = new SyncStep<int, int>(_ => throw new InvalidOperationException("boom"));
        var tryStep = step.TrySync();

        var result = tryStep.Invoke(5);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void TrySync_OperationCanceledException_IsNotCaught()
    {
        var step = new SyncStep<int, int>(_ => throw new OperationCanceledException());
        var tryStep = step.TrySync();

        var act = () => tryStep.Invoke(5);
        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void TryOption_PredicateTrue_ReturnsSome()
    {
        var step = new SyncStep<int, int>(x => x * 2);
        var optionStep = step.TryOption(x => x > 0);

        var result = optionStep.Invoke(5);
        result.HasValue.Should().BeTrue();
        result.Match(v => v, -1).Should().Be(10);
    }

    [Fact]
    public void TryOption_PredicateFalse_ReturnsNone()
    {
        var step = new SyncStep<int, int>(x => x * 2);
        var optionStep = step.TryOption(x => x > 100);

        var result = optionStep.Invoke(5);
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryOption_Exception_ReturnsNone()
    {
        var step = new SyncStep<int, int>(_ => throw new InvalidOperationException("boom"));
        var optionStep = step.TryOption(_ => true);

        var result = optionStep.Invoke(5);
        result.HasValue.Should().BeFalse();
    }
}
