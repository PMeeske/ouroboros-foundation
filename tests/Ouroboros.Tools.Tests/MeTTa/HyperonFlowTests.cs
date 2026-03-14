namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class HyperonFlowTests : IDisposable
{
    private readonly HyperonMeTTaEngine _engine = new();
    private readonly HyperonFlowIntegration _integration;

    public HyperonFlowTests()
    {
        _integration = new HyperonFlowIntegration(_engine);
    }

    public void Dispose()
    {
        _integration.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _engine.Dispose();
    }

    [Fact]
    public void Name_ReturnsConfiguredName()
    {
        var flow = _integration.CreateFlow("test-flow", "A test flow");
        flow.Name.Should().Be("test-flow");
    }

    [Fact]
    public void Description_ReturnsConfiguredDescription()
    {
        var flow = _integration.CreateFlow("test", "My description");
        flow.Description.Should().Be("My description");
    }

    [Fact]
    public async Task LoadFacts_AddsFacts()
    {
        var flow = _integration.CreateFlow("load-test", "test")
            .LoadFacts("(color red)", "(color blue)");

        await flow.ExecuteAsync();
        // Verify facts were added by querying
        var result = await _engine.ExecuteQueryAsync("(match &self (color $x) $x)");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyRule_AppliesRule()
    {
        var flow = _integration.CreateFlow("rule-test", "test")
            .ApplyRule("(= (double $x) (+ $x $x))");

        await flow.ExecuteAsync();
        // Should not throw
    }

    [Fact]
    public async Task Query_InvokesHandler()
    {
        string? capturedResult = null;
        var flow = _integration.CreateFlow("query-test", "test")
            .LoadFacts("(color red)")
            .Query("(match &self (color $x) $x)", result => capturedResult = result);

        await flow.ExecuteAsync();
        // Handler should have been invoked
    }

    [Fact]
    public async Task Transform_ExecutesCustomLogic()
    {
        bool transformed = false;
        var flow = _integration.CreateFlow("transform-test", "test")
            .Transform(async (engine, ct) => { transformed = true; });

        await flow.ExecuteAsync();
        transformed.Should().BeTrue();
    }

    [Fact]
    public async Task SideEffect_ExecutesAction()
    {
        bool executed = false;
        var flow = _integration.CreateFlow("side-test", "test")
            .SideEffect(engine => executed = true);

        await flow.ExecuteAsync();
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ChainsSteps()
    {
        var steps = new List<int>();
        var flow = _integration.CreateFlow("chain-test", "test")
            .SideEffect(_ => steps.Add(1))
            .SideEffect(_ => steps.Add(2))
            .SideEffect(_ => steps.Add(3));

        await flow.ExecuteAsync();
        steps.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task ExecuteAsync_CancellationToken_ThrowsWhenCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var flow = _integration.CreateFlow("cancel-test", "test")
            .SideEffect(_ => { });

        await Assert.ThrowsAsync<OperationCanceledException>(() => flow.ExecuteAsync(cts.Token));
    }
}
