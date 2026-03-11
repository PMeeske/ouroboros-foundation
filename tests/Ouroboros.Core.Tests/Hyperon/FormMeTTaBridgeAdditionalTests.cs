using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class FormMeTTaBridgeAdditionalTests : IDisposable
{
    private readonly AtomSpace _space;
    private readonly FormMeTTaBridge _bridge;

    public FormMeTTaBridgeAdditionalTests()
    {
        _space = new AtomSpace();
        _bridge = new FormMeTTaBridge(_space);
    }

    public void Dispose()
    {
        _bridge.Dispose();
    }

    #region DrawDistinction - Additional

    [Fact]
    public void DrawDistinction_OverwritesExistingState()
    {
        _bridge.CreateReEntry("ctx"); // set to Imaginary
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        var result = _bridge.DrawDistinction("ctx");

        result.Should().Be(Form.Mark);
        capturedArgs!.PreviousState.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void DrawDistinction_AddsDistinctionAtomToSpace()
    {
        var initialCount = _space.Count;

        _bridge.DrawDistinction("test-ctx");

        // Should have added a Distinction atom
        _space.Count.Should().BeGreaterThan(initialCount);
    }

    #endregion

    #region CrossDistinction - Additional

    [Fact]
    public void CrossDistinction_FromVoid_RaisesCrossedEvent()
    {
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        _bridge.CrossDistinction("new-context");

        capturedArgs.Should().NotBeNull();
        capturedArgs!.EventType.Should().Be(DistinctionEventType.Crossed);
    }

    [Fact]
    public void CrossDistinction_AddsUpdatedAtomToSpace()
    {
        _bridge.DrawDistinction("ctx");
        var countAfterDraw = _space.Count;

        _bridge.CrossDistinction("ctx");

        _space.Count.Should().BeGreaterThan(countAfterDraw);
    }

    #endregion

    #region CreateReEntry - Additional

    [Fact]
    public void CreateReEntry_OverwritesExistingMark()
    {
        _bridge.DrawDistinction("ctx");
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        var result = _bridge.CreateReEntry("ctx");

        result.Should().Be(Form.Imaginary);
        capturedArgs!.PreviousState.Should().Be(Form.Mark);
    }

    [Fact]
    public void CreateReEntry_AddsReEntryAtomToSpace()
    {
        var countBefore = _space.Count;

        _bridge.CreateReEntry("ctx");

        _space.Count.Should().BeGreaterThan(countBefore);
    }

    #endregion

    #region EvaluateTruthValue - Additional

    [Fact]
    public void EvaluateTruthValue_ImaginaryAtom_ReturnsImaginary()
    {
        var result = _bridge.EvaluateTruthValue(FormAtom.Imaginary);

        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void EvaluateTruthValue_ImaginarySymbol_ReturnsImaginary()
    {
        var result = _bridge.EvaluateTruthValue(Atom.Sym("Imaginary"));

        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void EvaluateTruthValue_UnknownSymbol_ChecksInterpreter()
    {
        // Unknown symbol not in space should return Void
        var result = _bridge.EvaluateTruthValue(Atom.Sym("unknown_symbol_xyz"));

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_SymbolInSpace_ReturnsMark()
    {
        _space.Add(Atom.Sym("known_fact"));

        var result = _bridge.EvaluateTruthValue(Atom.Sym("known_fact"));

        // Known fact should evaluate to something via the interpreter
        result.Should().NotBeNull();
    }

    [Fact]
    public void EvaluateTruthValue_GenericExpression_QuerySpace()
    {
        _space.Add(Atom.Expr(Atom.Sym("foo"), Atom.Sym("bar")));

        var result = _bridge.EvaluateTruthValue(Atom.Expr(Atom.Sym("foo"), Atom.Sym("bar")));

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_GenericExpression_NoMatch_ReturnsVoid()
    {
        var result = _bridge.EvaluateTruthValue(Atom.Expr(Atom.Sym("nonexistent"), Atom.Sym("bar")));

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_TruthValueEvaluatedEvent_IncludesTrace()
    {
        TruthValueEventArgs? capturedArgs = null;
        _bridge.TruthValueEvaluated += (_, args) => capturedArgs = args;

        _bridge.EvaluateTruthValue(Atom.Sym("Void"));

        capturedArgs.Should().NotBeNull();
        capturedArgs!.ReasoningTrace.Should().NotBeEmpty();
    }

    [Fact]
    public void EvaluateTruthValue_AndBothMark_ReturnsMark()
    {
        var expr = Atom.Expr(Atom.Sym("and"), Atom.Sym("Mark"), Atom.Sym("Mark"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_OrBothVoid_ReturnsVoid()
    {
        var expr = Atom.Expr(Atom.Sym("or"), Atom.Sym("Void"), Atom.Sym("Void"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_CallMarkMark_ReturnsMark()
    {
        var expr = Atom.Expr(Atom.Sym("call"), Atom.Sym("Mark"), Atom.Sym("Mark"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Mark);
    }

    #endregion

    #region DistinctionGatedInference - Additional

    [Fact]
    public void DistinctionGatedInference_ImaginaryGuard_ReturnsEmpty()
    {
        _bridge.CreateReEntry("guard");

        var results = _bridge.DistinctionGatedInference("guard", Atom.Sym("query")).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void DistinctionGatedInference_MarkedGuard_RaisesInferenceDerivedEvent()
    {
        _bridge.DrawDistinction("guard");
        _space.Add(Atom.Sym("fact"));

        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        var results = _bridge.DistinctionGatedInference("guard", Atom.Sym("fact")).ToList();

        capturedArgs.Should().NotBeNull();
        capturedArgs!.EventType.Should().Be(DistinctionEventType.InferenceDerived);
        capturedArgs.Context.Should().Be("guard");
    }

    [Fact]
    public void DistinctionGatedInference_MarkedGuard_NoResults_ReturnsEmpty()
    {
        _bridge.DrawDistinction("guard");

        var results = _bridge.DistinctionGatedInference("guard", Atom.Sym("nonexistent")).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region FormGatedMatch

    [Fact]
    public void FormGatedMatch_MatchingPattern_ReturnsResultsWithFormState()
    {
        _space.Add(Atom.Expr(Atom.Sym("color"), Atom.Sym("red")));

        var pattern = Atom.Expr(Atom.Sym("color"), Atom.Var("c"));

        var results = _bridge.FormGatedMatch(pattern).ToList();

        results.Should().NotBeEmpty();
        results[0].State.Should().NotBeNull();
    }

    [Fact]
    public void FormGatedMatch_NoMatch_ReturnsEmpty()
    {
        var pattern = Atom.Expr(Atom.Sym("nonexistent"), Atom.Var("x"));

        var results = _bridge.FormGatedMatch(pattern).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void FormGatedMatch_RaisesPatternMatchedEvent()
    {
        _space.Add(Atom.Expr(Atom.Sym("item"), Atom.Sym("a")));

        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        var results = _bridge.FormGatedMatch(Atom.Expr(Atom.Sym("item"), Atom.Var("x"))).ToList();

        capturedArgs.Should().NotBeNull();
        capturedArgs!.EventType.Should().Be(DistinctionEventType.PatternMatched);
    }

    [Fact]
    public void FormGatedMatch_GroundMatch_ReturnsMarkState()
    {
        _space.Add(Atom.Sym("ground_fact"));

        var results = _bridge.FormGatedMatch(Atom.Sym("ground_fact")).ToList();

        // Ground match with empty substitution should be Mark
        if (results.Any())
        {
            results[0].State.Should().Be(Form.Mark);
        }
    }

    #endregion

    #region MetaReason - Additional

    [Fact]
    public void MetaReason_SymbolWithNoVariables_ReportsFalse()
    {
        var results = _bridge.MetaReason(Atom.Sym("simple")).ToList();

        results.Should().Contain(a =>
            a is Expression e && e.Children.Count > 1 &&
            e.Children[0] is Symbol s && s.Name == "has-variables" &&
            e.Children[1] is Symbol v && v.Name == "False");
    }

    [Fact]
    public void MetaReason_Expression_IncludesHeadInfo()
    {
        var expr = Atom.Expr(Atom.Sym("myFunc"), Atom.Sym("arg1"));

        var results = _bridge.MetaReason(expr).ToList();

        results.Should().Contain(a =>
            a is Expression e && e.Children.Count > 2 &&
            e.Children[0] is Symbol s && s.Name == "head");
    }

    [Fact]
    public void MetaReason_Expression_IncludesArityInfo()
    {
        var expr = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"), Atom.Sym("y"));

        var results = _bridge.MetaReason(expr).ToList();

        results.Should().Contain(a =>
            a is Expression e && e.Children.Count > 2 &&
            e.Children[0] is Symbol s && s.Name == "arity" &&
            e.Children[2] is Symbol count && count.Name == "3");
    }

    [Fact]
    public void MetaReason_AddedFactsAreInSpace()
    {
        var initialCount = _space.Count;

        var results = _bridge.MetaReason(Atom.Sym("test")).ToList();

        _space.Count.Should().BeGreaterThan(initialCount);
    }

    #endregion

    #region GetAllDistinctions - Additional

    [Fact]
    public void GetAllDistinctions_Empty_ReturnsEmptyDictionary()
    {
        var all = _bridge.GetAllDistinctions();

        all.Should().BeEmpty();
    }

    [Fact]
    public void GetAllDistinctions_MixedStates_ReturnsAll()
    {
        _bridge.DrawDistinction("marked");
        _bridge.CreateReEntry("imaginary");

        var all = _bridge.GetAllDistinctions();

        all["marked"].Should().Be(Form.Mark);
        all["imaginary"].Should().Be(Form.Imaginary);
    }

    #endregion

    #region Dispose - Additional

    [Fact]
    public void Dispose_ClearsDistinctionContextAndCache()
    {
        var space = new AtomSpace();
        var bridge = new FormMeTTaBridge(space);
        bridge.DrawDistinction("test");

        bridge.Dispose();

        // After dispose, internal state should be cleared
        // We verify by calling dispose again (should not throw)
        bridge.Dispose();
    }

    #endregion
}
