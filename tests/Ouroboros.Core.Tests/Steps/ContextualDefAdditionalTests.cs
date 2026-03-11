namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class ContextualDefAdditionalTests
{
    [Fact]
    public async Task LiftPure_ChainedWithPipe_ComposesProperly()
    {
        var def = ContextualDef.LiftPure<int, int, object>(x => x + 1, "added")
            .Pipe<string>(x => x.ToString(), "stringified");

        var (result, logs) = await def.InvokeAsync(4, new object());
        result.Should().Be("5");
        logs.Should().Contain("added");
        logs.Should().Contain("stringified");
    }

    [Fact]
    public async Task FromPure_WithNullLog_EmitsNoLog()
    {
        Step<int, string> step = x => Task.FromResult(x.ToString());
        var def = ContextualDef.FromPure<int, string, object>(step, null);

        var (result, logs) = await def.InvokeAsync(42, new object());
        result.Should().Be("42");
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task FromContext_ChainedWithPipe_ComposesProperly()
    {
        Step<string, Step<int, int>> ctxStep = ctx =>
            Task.FromResult<Step<int, int>>(x =>
                Task.FromResult(x + ctx.Length));

        var def = ContextualDef.FromContext<int, int, string>(ctxStep)
            .Pipe<string>(x => x.ToString());

        var (result, _) = await def.InvokeAsync(5, "abc");
        result.Should().Be("8"); // 5 + 3 ("abc".Length)
    }

    [Fact]
    public async Task Identity_WithNullLog_EmitsNoLog()
    {
        var def = ContextualDef.Identity<int, object>(null);
        var (result, logs) = await def.InvokeAsync(42, new object());

        result.Should().Be(42);
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task LiftPure_WithNullLog_EmitsNoLog()
    {
        var def = ContextualDef.LiftPure<int, string, object>(x => x.ToString(), null);
        var (result, logs) = await def.InvokeAsync(42, new object());

        result.Should().Be("42");
        logs.Should().BeEmpty();
    }
}
