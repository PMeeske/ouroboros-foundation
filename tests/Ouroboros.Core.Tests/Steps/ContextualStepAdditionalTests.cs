namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class ContextualStepAdditionalTests
{
    [Fact]
    public async Task LiftPure_IgnoresContext()
    {
        var step = ContextualStep.LiftPure<int, string, string>(x => x.ToString());

        var (result1, _) = await step(42, "ctx1");
        var (result2, _) = await step(42, "ctx2");

        result1.Should().Be(result2);
    }

    [Fact]
    public async Task FromPure_IgnoresContext()
    {
        Step<int, string> pureStep = x => Task.FromResult(x.ToString());
        var step = ContextualStep.FromPure<int, string, string>(pureStep);

        var (result1, _) = await step(42, "ctx1");
        var (result2, _) = await step(42, "ctx2");

        result1.Should().Be(result2);
    }

    [Fact]
    public async Task FromContext_UsesContextToResolveStep()
    {
        Step<int, Step<string, string>> contextStep = multiplier =>
            Task.FromResult<Step<string, string>>(s =>
                Task.FromResult(string.Concat(Enumerable.Repeat(s, multiplier))));

        var step = ContextualStep.FromContext<string, string, int>(contextStep);

        var (result, logs) = await step("ab", 3);
        result.Should().Be("ababab");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task Identity_WithNullLog_EmitsNoLogs()
    {
        var step = ContextualStep.Identity<int, object>(null);
        var (result, logs) = await step(42, new object());

        result.Should().Be(42);
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task LiftPure_WithNullLog_EmitsNoLogs()
    {
        var step = ContextualStep.LiftPure<int, string, object>(x => x.ToString(), null);
        var (_, logs) = await step(42, new object());

        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task FromPure_WithNullLog_EmitsNoLogs()
    {
        Step<int, string> pureStep = x => Task.FromResult(x.ToString());
        var step = ContextualStep.FromPure<int, string, object>(pureStep, null);

        var (_, logs) = await step(42, new object());
        logs.Should().BeEmpty();
    }
}
