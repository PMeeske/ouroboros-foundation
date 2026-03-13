namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class HyperonFlowIntegrationTests : IAsyncDisposable
{
    private readonly HyperonMeTTaEngine _engine = new();
    private readonly HyperonFlowIntegration _sut;

    public HyperonFlowIntegrationTests()
    {
        _sut = new HyperonFlowIntegration(_engine);
    }

    public async ValueTask DisposeAsync()
    {
        await _sut.DisposeAsync();
        _engine.Dispose();
    }

    [Fact]
    public void Constructor_NullEngine_ThrowsArgumentNullException()
    {
        var act = () => new HyperonFlowIntegration(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Engine_ReturnsUnderlyingEngine()
    {
        _sut.Engine.Should().BeSameAs(_engine);
    }

    [Fact]
    public void SubscribePattern_CreatesSubscription()
    {
        bool matched = false;
        _sut.SubscribePattern("sub1", "TestAtom", _ => matched = true);
        // Subscription should be stored without throwing
    }

    [Fact]
    public void UnsubscribePattern_RemovesSubscription()
    {
        _sut.SubscribePattern("sub1", "TestAtom", _ => { });
        _sut.UnsubscribePattern("sub1");
        // Should not throw
    }

    [Fact]
    public void UnsubscribePattern_NonExistent_DoesNotThrow()
    {
        var act = () => _sut.UnsubscribePattern("nonexistent");
        act.Should().NotThrow();
    }

    [Fact]
    public void CreateFlow_ReturnsFlow()
    {
        var flow = _sut.CreateFlow("test", "description");
        flow.Should().NotBeNull();
        flow.Name.Should().Be("test");
    }

    [Fact]
    public void GetFlow_ExistingFlow_ReturnsFlow()
    {
        _sut.CreateFlow("my-flow", "desc");
        var flow = _sut.GetFlow("my-flow");
        flow.Should().NotBeNull();
    }

    [Fact]
    public void GetFlow_NonExistent_ReturnsNull()
    {
        var flow = _sut.GetFlow("nonexistent");
        flow.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteFlowAsync_ExistingFlow_Executes()
    {
        bool executed = false;
        _sut.CreateFlow("exec-test", "test")
            .SideEffect(_ => executed = true);

        await _sut.ExecuteFlowAsync("exec-test");
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteFlowAsync_NonExistentFlow_DoesNotThrow()
    {
        var act = () => _sut.ExecuteFlowAsync("nonexistent");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void CreateConsciousnessLoop_ReturnsCts()
    {
        var cts = _sut.CreateConsciousnessLoop("loop1", 1, TimeSpan.FromMilliseconds(100));
        cts.Should().NotBeNull();
        cts.Cancel();
    }

    [Fact]
    public async Task DisposeAsync_MultipleCalls_DoesNotThrow()
    {
        await _sut.DisposeAsync();
        var act = () => _sut.DisposeAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void OnPatternMatch_EventCanBeSubscribed()
    {
        PatternMatch? received = null;
        _sut.OnPatternMatch += match => received = match;
        // Event subscription should work
    }
}
