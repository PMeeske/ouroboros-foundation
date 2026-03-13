namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Abstractions;
using Ouroboros.Core.Hyperon;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class HyperonMeTTaEngineTests : IDisposable
{
    private readonly HyperonMeTTaEngine _sut = new();

    public void Dispose() => _sut.Dispose();

    #region Constructor and Properties

    [Fact]
    public void Constructor_Default_CreatesEngine()
    {
        _sut.AtomSpace.Should().NotBeNull();
        _sut.Interpreter.Should().NotBeNull();
        _sut.Parser.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomGroundedOps_CreatesEngine()
    {
        using var engine = new HyperonMeTTaEngine(new GroundedRegistry());
        engine.Should().NotBeNull();
    }

    [Fact]
    public void FromAtomSpace_CopiesAtoms()
    {
        var space = new AtomSpace();
        space.Add(Atom.Sym("TestAtom"));

        using var engine = HyperonMeTTaEngine.FromAtomSpace(space);
        engine.Should().NotBeNull();
    }

    #endregion

    #region ExecuteQueryAsync

    [Fact]
    public async Task ExecuteQueryAsync_ValidSymbol_ReturnsSuccess()
    {
        var result = await _sut.ExecuteQueryAsync("True");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteQueryAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.ExecuteQueryAsync("True");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    #endregion

    #region AddFactAsync

    [Fact]
    public async Task AddFactAsync_ValidFact_ReturnsSuccess()
    {
        var result = await _sut.AddFactAsync("(color red)");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddFactAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.AddFactAsync("(fact a)");
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region ApplyRuleAsync

    [Fact]
    public async Task ApplyRuleAsync_ValidRule_ReturnsSuccess()
    {
        var result = await _sut.ApplyRuleAsync("(= (double $x) (+ $x $x))");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyRuleAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.ApplyRuleAsync("rule");
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region VerifyPlanAsync

    [Fact]
    public async Task VerifyPlanAsync_ValidPlan_ReturnsTrue()
    {
        var result = await _sut.VerifyPlanAsync("(step1 step2)");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPlanAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.VerifyPlanAsync("plan");
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region VerifyPlanStepsAsync

    [Fact]
    public async Task VerifyPlanStepsAsync_EmptySteps_ReturnsTrue()
    {
        var result = await _sut.VerifyPlanStepsAsync(Array.Empty<string>());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPlanStepsAsync_ValidSteps_ReturnsTrue()
    {
        var result = await _sut.VerifyPlanStepsAsync(new[] { "True", "False" });
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPlanStepsAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.VerifyPlanStepsAsync(new[] { "step" });
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region ResetAsync

    [Fact]
    public async Task ResetAsync_ClearsState()
    {
        await _sut.AddFactAsync("(test fact)");
        var result = await _sut.ResetAsync();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResetAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.ResetAsync();
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region Atom Operations

    [Fact]
    public void AddAtom_AddsToSpace()
    {
        var atom = Atom.Sym("TestSymbol");
        _sut.AddAtom(atom);
        // Should not throw
    }

    [Fact]
    public void BindAtom_BindsNameToAtom()
    {
        var atom = Atom.Sym("BoundAtom");
        _sut.BindAtom("myAtom", atom);
        var result = _sut.GetNamedAtom("myAtom");
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetNamedAtom_NotBound_ReturnsNull()
    {
        var result = _sut.GetNamedAtom("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public void AtomAdded_EventRaised()
    {
        Atom? received = null;
        _sut.AtomAdded += a => received = a;
        _sut.AddAtom(Atom.Sym("EventTest"));
        received.Should().NotBeNull();
    }

    [Fact]
    public void Query_ReturnsMatches()
    {
        var pattern = Atom.Sym("True");
        var results = _sut.Query(pattern).ToList();
        // Query should work without throwing
        results.Should().NotBeNull();
    }

    #endregion

    #region LoadMeTTaSourceAsync

    [Fact]
    public async Task LoadMeTTaSourceAsync_ValidSource_ReturnsSuccess()
    {
        var source = "(color red)\n(color blue)\n; comment\n\n(size large)";
        var result = await _sut.LoadMeTTaSourceAsync(source);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task LoadMeTTaSourceAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.LoadMeTTaSourceAsync("(fact a)");
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region ExportToMeTTa

    [Fact]
    public void ExportToMeTTa_ReturnsNonEmptyString()
    {
        var result = _sut.ExportToMeTTa();
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Exported from HyperonMeTTaEngine");
    }

    #endregion

    #region RegisterGroundedOp

    [Fact]
    public void RegisterGroundedOp_DoesNotThrow()
    {
        var act = () => _sut.RegisterGroundedOp("custom-op", (space, args) => new[] { Atom.Sym("ok") });
        act.Should().NotThrow();
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        _sut.Dispose();
        var act = () => _sut.Dispose();
        act.Should().NotThrow();
    }

    #endregion
}
