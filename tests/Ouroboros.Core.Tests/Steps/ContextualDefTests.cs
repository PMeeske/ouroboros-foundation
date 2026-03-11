using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class ContextualDefTests
{
    [Fact]
    public async Task LiftPure_CreatesContextualStepDefinition()
    {
        var def = ContextualDef.LiftPure<int, string, object>(x => x.ToString());
        var (result, logs) = await def.InvokeAsync(42, new object());

        result.Should().Be("42");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task LiftPure_WithLog_EmitsLog()
    {
        var def = ContextualDef.LiftPure<int, string, object>(x => x.ToString(), "converted");
        var (result, logs) = await def.InvokeAsync(42, new object());

        result.Should().Be("42");
        logs.Should().ContainSingle("converted");
    }

    [Fact]
    public async Task FromPure_CreatesContextualStepDefinition()
    {
        Step<int, string> step = x => Task.FromResult(x.ToString());
        var def = ContextualDef.FromPure<int, string, object>(step);

        var (result, logs) = await def.InvokeAsync(7, new object());
        result.Should().Be("7");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task FromPure_WithLog_EmitsLog()
    {
        Step<int, string> step = x => Task.FromResult(x.ToString());
        var def = ContextualDef.FromPure<int, string, object>(step, "step-log");

        var (result, logs) = await def.InvokeAsync(7, new object());
        result.Should().Be("7");
        logs.Should().ContainSingle("step-log");
    }

    [Fact]
    public async Task FromContext_CreatesContextDependentDefinition()
    {
        Step<string, Step<int, string>> ctxStep = ctx =>
            Task.FromResult<Step<int, string>>(x => Task.FromResult($"{ctx}:{x}"));

        var def = ContextualDef.FromContext<int, string, string>(ctxStep);
        var (result, logs) = await def.InvokeAsync(5, "prefix");

        result.Should().Be("prefix:5");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task Identity_ReturnsInputUnchanged()
    {
        var def = ContextualDef.Identity<int, object>();
        var (result, logs) = await def.InvokeAsync(42, new object());

        result.Should().Be(42);
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task Identity_WithLog_EmitsLog()
    {
        var def = ContextualDef.Identity<int, object>("identity-log");
        var (result, logs) = await def.InvokeAsync(42, new object());

        result.Should().Be(42);
        logs.Should().ContainSingle("identity-log");
    }
}
