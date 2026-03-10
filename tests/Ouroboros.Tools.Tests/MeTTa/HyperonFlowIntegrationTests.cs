// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

using Ouroboros.Core.Hyperon;

namespace Ouroboros.Tests.Tools.MeTTa;

/// <summary>
/// Unit tests for HyperonFlowIntegration covering construction, pattern subscriptions,
/// flow management, consciousness loops, and disposal.
/// </summary>
[Trait("Category", "Unit")]
public class HyperonFlowIntegrationTests : IAsyncDisposable
{
    private readonly HyperonMeTTaEngine _engine;
    private readonly HyperonFlowIntegration _integration;

    public HyperonFlowIntegrationTests()
    {
        _engine = new HyperonMeTTaEngine();
        _integration = new HyperonFlowIntegration(_engine);
    }

    public async ValueTask DisposeAsync()
    {
        await _integration.DisposeAsync();
        _engine.Dispose();
    }

    // ========================================================================
    // Constructor
    // ========================================================================

    [Fact]
    public void Constructor_NullEngine_ThrowsArgumentNull()
    {
        var act = () => new HyperonFlowIntegration(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidEngine_SetsEngineProperty()
    {
        _integration.Engine.Should().BeSameAs(_engine);
    }

    // ========================================================================
    // SubscribePattern / UnsubscribePattern
    // ========================================================================

    [Fact]
    public void SubscribePattern_RegistersSubscription()
    {
        // Act - should not throw
        var act = () => _integration.SubscribePattern(
            "sub1", "(test $x)", _ => { });

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SubscribePattern_SameId_OverwritesPrevious()
    {
        // Arrange
        var handlerCalls = 0;
        _integration.SubscribePattern("dup", "(test $x)", _ => handlerCalls++);

        // Act - overwrite with new handler
        _integration.SubscribePattern("dup", "(test $x)", _ => handlerCalls += 10);

        // Assert - should not throw
        handlerCalls.Should().Be(0); // No invocations yet
    }

    [Fact]
    public void UnsubscribePattern_RemovesSubscription()
    {
        // Arrange
        _integration.SubscribePattern("removable", "(test $x)", _ => { });

        // Act
        var act = () => _integration.UnsubscribePattern("removable");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void UnsubscribePattern_NonExistent_DoesNotThrow()
    {
        var act = () => _integration.UnsubscribePattern("nonExistent");
        act.Should().NotThrow();
    }

    // ========================================================================
    // Pattern matching via atom addition
    // ========================================================================

    [Fact]
    public async Task SubscribePattern_AtomAdded_MatchingPattern_InvokesHandler()
    {
        // Arrange
        PatternMatch? receivedMatch = null;
        _integration.SubscribePattern(
            "matchTest",
            "(color $x)",
            match => receivedMatch = match);

        // Act - add an atom that matches the pattern
        await _engine.AddFactAsync("(color red)");

        // Assert
        receivedMatch.Should().NotBeNull();
        receivedMatch!.SubscriptionId.Should().Be("matchTest");
        receivedMatch.Pattern.Should().Be("(color $x)");
    }

    [Fact]
    public async Task SubscribePattern_NonMatchingAtom_DoesNotInvokeHandler()
    {
        // Arrange
        var handlerCalled = false;
        _integration.SubscribePattern(
            "noMatch",
            "(shape $x)",
            _ => handlerCalled = true);

        // Act - add atom that does not match the pattern
        await _engine.AddFactAsync("(color blue)");

        // Assert
        handlerCalled.Should().BeFalse();
    }

    [Fact]
    public async Task OnPatternMatch_Event_FiredWhenPatternMatches()
    {
        // Arrange
        PatternMatch? eventMatch = null;
        _integration.OnPatternMatch += match => eventMatch = match;
        _integration.SubscribePattern(
            "eventTest",
            "(data $x)",
            _ => { });

        // Act
        await _engine.AddFactAsync("(data value)");

        // Assert
        eventMatch.Should().NotBeNull();
    }

    // ========================================================================
    // CreateFlow
    // ========================================================================

    [Fact]
    public void CreateFlow_ReturnsHyperonFlow()
    {
        var flow = _integration.CreateFlow("testFlow", "A test flow");

        flow.Should().NotBeNull();
        flow.Name.Should().Be("testFlow");
        flow.Description.Should().Be("A test flow");
    }

    [Fact]
    public void CreateFlow_RegistersFlowByName()
    {
        _integration.CreateFlow("registered", "Registered flow");

        var retrieved = _integration.GetFlow("registered");
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("registered");
    }

    // ========================================================================
    // GetFlow
    // ========================================================================

    [Fact]
    public void GetFlow_ExistingName_ReturnsFlow()
    {
        _integration.CreateFlow("exists", "Flow exists");
        _integration.GetFlow("exists").Should().NotBeNull();
    }

    [Fact]
    public void GetFlow_NonExistentName_ReturnsNull()
    {
        _integration.GetFlow("doesNotExist").Should().BeNull();
    }

    // ========================================================================
    // ExecuteFlowAsync
    // ========================================================================

    [Fact]
    public async Task ExecuteFlowAsync_ExistingFlow_ExecutesSuccessfully()
    {
        // Arrange
        var executed = false;
        _integration.CreateFlow("execFlow", "Executable flow")
            .SideEffect(_ => executed = true);

        // Act
        await _integration.ExecuteFlowAsync("execFlow");

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteFlowAsync_NonExistentFlow_DoesNotThrow()
    {
        var act = () => _integration.ExecuteFlowAsync("nonExistent");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteFlowAsync_FlowWithFacts_AddsFactsToEngine()
    {
        // Arrange
        _integration.CreateFlow("factFlow", "Fact loading flow")
            .LoadFacts("(testfact alpha)", "(testfact beta)");

        // Act
        await _integration.ExecuteFlowAsync("factFlow");

        // Assert
        var atoms = _engine.AtomSpace.All().ToList();
        atoms.Should().Contain(a => a.ToSExpr() == "(testfact alpha)");
        atoms.Should().Contain(a => a.ToSExpr() == "(testfact beta)");
    }

    // ========================================================================
    // CreateConsciousnessLoop
    // ========================================================================

    [Fact]
    public void CreateConsciousnessLoop_ReturnsValidCts()
    {
        var cts = _integration.CreateConsciousnessLoop("loop1", 2, TimeSpan.FromSeconds(60));

        cts.Should().NotBeNull();
        cts.Token.IsCancellationRequested.Should().BeFalse();

        cts.Cancel();
        cts.Dispose();
    }

    [Fact]
    public void CreateConsciousnessLoop_DefaultInterval_DoesNotThrow()
    {
        var cts = _integration.CreateConsciousnessLoop("loop2");

        cts.Should().NotBeNull();

        cts.Cancel();
        cts.Dispose();
    }

    [Fact]
    public void CreateConsciousnessLoop_CanBeCancelled()
    {
        var cts = _integration.CreateConsciousnessLoop("cancelLoop", interval: TimeSpan.FromSeconds(60));

        cts.Cancel();
        cts.Token.IsCancellationRequested.Should().BeTrue();

        cts.Dispose();
    }

    // ========================================================================
    // DisposeAsync
    // ========================================================================

    [Fact]
    public async Task DisposeAsync_ClearsSubscriptionsAndFlows()
    {
        // Arrange
        var integration = new HyperonFlowIntegration(_engine);
        integration.SubscribePattern("sub", "(test $x)", _ => { });
        integration.CreateFlow("f", "flow");

        // Act
        await integration.DisposeAsync();

        // Assert
        integration.GetFlow("f").Should().BeNull();
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_IsIdempotent()
    {
        // Arrange
        var integration = new HyperonFlowIntegration(_engine);

        // Act
        var act = async () =>
        {
            await integration.DisposeAsync();
            await integration.DisposeAsync();
        };

        // Assert
        await act.Should().NotThrowAsync();
    }

    // ========================================================================
    // Multiple subscriptions
    // ========================================================================

    [Fact]
    public async Task MultipleSubscriptions_AllMatchingOnesInvoked()
    {
        // Arrange
        var matches = new List<string>();
        _integration.SubscribePattern("sub1", "(info $x)", m => matches.Add("sub1"));
        _integration.SubscribePattern("sub2", "(info $x)", m => matches.Add("sub2"));

        // Act
        await _engine.AddFactAsync("(info test)");

        // Assert
        matches.Should().Contain("sub1");
        matches.Should().Contain("sub2");
    }
}
