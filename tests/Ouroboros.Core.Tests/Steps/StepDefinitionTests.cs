using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class StepDefinitionTests
{
    [Fact]
    public void Constructor_WithAsyncStep_NullThrows()
    {
        Step<int, int> step = null!;
        var act = () => new StepDefinition<int, int>(step);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithSyncFunc_NullThrows()
    {
        Func<int, int> func = null!;
        var act = () => new StepDefinition<int, int>(func);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Constructor_WithAsyncStep_WrapsCorrectly()
    {
        Step<int, int> step = x => Task.FromResult(x + 1);
        var def = new StepDefinition<int, int>(step);

        var result = await def.Build()(5);
        result.Should().Be(6);
    }

    [Fact]
    public async Task Constructor_WithSyncFunc_WrapsCorrectly()
    {
        var def = new StepDefinition<int, int>(x => x + 1);

        var result = await def.Build()(5);
        result.Should().Be(6);
    }

    [Fact]
    public async Task Pipe_AsyncStep_ComposesPipeline()
    {
        var def = new StepDefinition<int, int>(x => x + 1);
        Step<int, string> next = x => Task.FromResult(x.ToString());

        var composed = def.Pipe(next);
        var result = await composed.Build()(9);

        result.Should().Be("10");
    }

    [Fact]
    public void Pipe_AsyncStep_NullThrows()
    {
        var def = new StepDefinition<int, int>(x => x + 1);
        Step<int, string> next = null!;

        var act = () => def.Pipe(next);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Pipe_SyncFunc_ComposesPipeline()
    {
        var def = new StepDefinition<int, int>(x => x + 1);
        var composed = def.Pipe<string>(x => x.ToString());

        var result = await composed.Build()(9);
        result.Should().Be("10");
    }

    [Fact]
    public void Pipe_SyncFunc_NullThrows()
    {
        var def = new StepDefinition<int, int>(x => x + 1);
        Func<int, string> func = null!;

        var act = () => def.Pipe(func);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task PipeOperator_AsyncStep_ComposesPipeline()
    {
        var def = new StepDefinition<int, int>(x => x + 1);
        Step<int, int> next = x => Task.FromResult(x * 2);

        var composed = def | next;
        var result = await composed.Build()(4);

        result.Should().Be(10); // (4 + 1) * 2
    }

    [Fact]
    public async Task PipeOperator_SyncFunc_ComposesPipeline()
    {
        var def = new StepDefinition<int, int>(x => x + 1);
        Func<int, int> func = x => x * 3;

        var composed = def | func;
        var result = await composed.Build()(4);

        result.Should().Be(15); // (4 + 1) * 3
    }

    [Fact]
    public async Task MultiplePipes_ChainCorrectly()
    {
        var def = new StepDefinition<int, int>(x => x + 1);
        var composed = def
            .Pipe<int>(x => x * 2)
            .Pipe<string>(x => $"answer={x}");

        var result = await composed.Build()(4);
        result.Should().Be("answer=10"); // (4+1)*2 = 10
    }
}
