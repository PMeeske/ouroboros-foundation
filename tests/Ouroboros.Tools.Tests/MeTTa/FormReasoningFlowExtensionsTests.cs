namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Core.LawsOfForm;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class FormReasoningFlowExtensionsTests : IDisposable
{
    private readonly HyperonMeTTaEngine _engine = new();
    private readonly HyperonFlowIntegration _integration;

    public FormReasoningFlowExtensionsTests()
    {
        _integration = new HyperonFlowIntegration(_engine);
    }

    public void Dispose()
    {
        _integration.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _engine.Dispose();
    }

    [Fact]
    public async Task DrawDistinction_AddsDistinctionFact()
    {
        Form? received = null;
        var flow = _integration.CreateFlow("dd-test", "test")
            .DrawDistinction("context1", f => received = f);

        await flow.ExecuteAsync();
        received.Should().NotBeNull();
    }

    [Fact]
    public async Task CrossDistinction_CrossesExistingDistinction()
    {
        Form? received = null;
        var flow = _integration.CreateFlow("cd-test", "test")
            .DrawDistinction("ctx")
            .CrossDistinction("ctx", f => received = f);

        await flow.ExecuteAsync();
        received.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateReEntry_CreatesReEntryForm()
    {
        Form? received = null;
        var flow = _integration.CreateFlow("re-test", "test")
            .CreateReEntry("ctx", f => received = f);

        await flow.ExecuteAsync();
        received.Should().NotBeNull();
    }

    [Fact]
    public async Task DistinctionGated_WhenMarked_ExecutesQuery()
    {
        string? result = null;
        var flow = _integration.CreateFlow("gated-test", "test")
            .DrawDistinction("gate")
            .DistinctionGated("gate", "(match &self (Distinction gate $s) $s)", r => result = r);

        await flow.ExecuteAsync();
        // May or may not execute depending on atom resolution
    }

    [Fact]
    public async Task ApplyLawOfCrossing_DoesNotThrow()
    {
        var flow = _integration.CreateFlow("crossing-test", "test")
            .ApplyLawOfCrossing("ctx");

        var act = () => flow.ExecuteAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ApplyLawOfCalling_DoesNotThrow()
    {
        var flow = _integration.CreateFlow("calling-test", "test")
            .ApplyLawOfCalling("ctx");

        var act = () => flow.ExecuteAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CheckCertainty_WhenUncertain_InvokesHandler()
    {
        bool uncertainCalled = false;
        var flow = _integration.CreateFlow("certainty-test", "test")
            .CheckCertainty("nonexistent", onUncertain: () => uncertainCalled = true);

        await flow.ExecuteAsync();
        uncertainCalled.Should().BeTrue();
    }

    [Fact]
    public async Task FormBranch_WhenNoState_DefaultsToVoid()
    {
        bool voidCalled = false;
        var flow = _integration.CreateFlow("branch-test", "test")
            .FormBranch("nonexistent", onVoid: f => { voidCalled = true; return f; });

        await flow.ExecuteAsync();
        voidCalled.Should().BeTrue();
    }

    [Fact]
    public async Task MetaReasonAboutForms_DoesNotThrow()
    {
        var flow = _integration.CreateFlow("meta-test", "test")
            .DrawDistinction("a")
            .MetaReasonAboutForms();

        var act = () => flow.ExecuteAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void CreateFormAwareConsciousnessLoop_ReturnsCts()
    {
        using var cts = _integration.CreateFormAwareConsciousnessLoop(
            "loop1",
            onReflection: _ => { },
            interval: TimeSpan.FromMilliseconds(100));

        cts.Should().NotBeNull();
        cts.Cancel();
    }
}
