using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public class ContextualStepTests
{
    [Fact]
    public async Task LiftPure_AppliesFunction()
    {
        var step = ContextualStep.LiftPure<int, string, object>(x => x.ToString());
        var (result, logs) = await step(42, new object());

        result.Should().Be("42");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task LiftPure_WithLog_EmitsLog()
    {
        var step = ContextualStep.LiftPure<int, string, object>(x => x.ToString(), "converted");
        var (result, logs) = await step(42, new object());

        result.Should().Be("42");
        logs.Should().ContainSingle("converted");
    }

    [Fact]
    public void LiftPure_NullFunc_ThrowsArgumentNullException()
    {
        var act = () => ContextualStep.LiftPure<int, string, object>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task FromPure_WrapsAsyncStep()
    {
        Step<int, string> pureStep = x => Task.FromResult(x.ToString());
        var step = ContextualStep.FromPure<int, string, object>(pureStep);

        var (result, logs) = await step(42, new object());
        result.Should().Be("42");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task FromPure_WithLog_EmitsLog()
    {
        Step<int, string> pureStep = x => Task.FromResult(x.ToString());
        var step = ContextualStep.FromPure<int, string, object>(pureStep, "step executed");

        var (result, logs) = await step(42, new object());
        result.Should().Be("42");
        logs.Should().ContainSingle("step executed");
    }

    [Fact]
    public void FromPure_NullStep_ThrowsArgumentNullException()
    {
        var act = () => ContextualStep.FromPure<int, string, object>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task FromContext_ResolvesInnerStepFromContext()
    {
        Step<string, Step<int, string>> contextStep = ctx =>
            Task.FromResult<Step<int, string>>(x => Task.FromResult($"{ctx}:{x}"));

        var step = ContextualStep.FromContext<int, string, string>(contextStep);
        var (result, logs) = await step(42, "prefix");

        result.Should().Be("prefix:42");
        logs.Should().BeEmpty();
    }

    [Fact]
    public void FromContext_NullStep_ThrowsArgumentNullException()
    {
        var act = () => ContextualStep.FromContext<int, string, string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Identity_ReturnsInputUnchanged()
    {
        var step = ContextualStep.Identity<int, object>();
        var (result, logs) = await step(42, new object());

        result.Should().Be(42);
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task Identity_WithLog_EmitsLog()
    {
        var step = ContextualStep.Identity<int, object>("identity");
        var (result, logs) = await step(42, new object());

        result.Should().Be(42);
        logs.Should().ContainSingle("identity");
    }
}
