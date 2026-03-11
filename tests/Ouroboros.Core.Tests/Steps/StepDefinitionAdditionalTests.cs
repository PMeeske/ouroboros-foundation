namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class StepDefinitionAdditionalTests
{
    [Fact]
    public async Task Build_ReturnsWorkingDelegate()
    {
        var def = new StepDefinition<string, int>(s => s.Length);
        var step = def.Build();

        var result = await step("hello");
        result.Should().Be(5);
    }

    [Fact]
    public async Task Pipe_AsyncStep_MultiplePipes()
    {
        Step<int, int> addOne = x => Task.FromResult(x + 1);
        Step<int, int> multiplyTwo = x => Task.FromResult(x * 2);
        Step<int, string> toString = x => Task.FromResult(x.ToString());

        var def = new StepDefinition<int, int>(addOne);
        var composed = def.Pipe(multiplyTwo).Pipe(toString);
        var result = await composed.Build()(4);

        result.Should().Be("10"); // (4+1)*2 = 10
    }

    [Fact]
    public async Task PipeOperator_AsyncStep_Chainable()
    {
        var def = new StepDefinition<int, int>(x => x + 1);
        Step<int, int> doubler = x => Task.FromResult(x * 2);
        Step<int, int> addTen = x => Task.FromResult(x + 10);

        var composed = (def | doubler) | addTen;
        var result = await composed.Build()(4);

        result.Should().Be(20); // ((4+1)*2)+10 = 20
    }

    [Fact]
    public async Task PipeOperator_SyncFunc_Chainable()
    {
        var def = new StepDefinition<int, int>(x => x + 1);
        Func<int, int> doubler = x => x * 2;
        Func<int, int> addTen = x => x + 10;

        var composed = (def | doubler) | addTen;
        var result = await composed.Build()(4);

        result.Should().Be(20); // ((4+1)*2)+10 = 20
    }

    [Fact]
    public async Task Pipe_SyncFunc_PreservesAsync()
    {
        var def = new StepDefinition<string, string>(s => s.ToUpperInvariant());
        var composed = def.Pipe<int>(s => s.Length);

        var result = await composed.Build()("hello");
        result.Should().Be(5);
    }
}
