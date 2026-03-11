using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class FormMeTTaBridgeTests : IDisposable
{
    private readonly AtomSpace _space;
    private readonly FormMeTTaBridge _bridge;

    public FormMeTTaBridgeTests()
    {
        _space = new AtomSpace();
        _bridge = new FormMeTTaBridge(_space);
    }

    public void Dispose()
    {
        _bridge.Dispose();
    }

    #region Constructor

    [Fact]
    public void Constructor_NullSpace_ThrowsArgumentNullException()
    {
        Action act = () => new FormMeTTaBridge(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("space");
    }

    [Fact]
    public void Constructor_InitializesFormAxioms()
    {
        // The bridge adds axioms to the space during construction
        _space.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Constructor_WithCustomGroundedOps_UsesProvided()
    {
        var registry = GroundedRegistry.CreateStandard();
        var space = new AtomSpace();

        using var bridge = new FormMeTTaBridge(space, registry);

        bridge.Should().NotBeNull();
    }

    #endregion

    #region DrawDistinction

    [Fact]
    public void DrawDistinction_ReturnsMark()
    {
        var result = _bridge.DrawDistinction("test");

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void DrawDistinction_SetsContextState()
    {
        _bridge.DrawDistinction("context1");

        _bridge.GetFormState("context1").Should().Be(Form.Mark);
    }

    [Fact]
    public void DrawDistinction_WithReason_SetsState()
    {
        var reason = Atom.Sym("because");
        var result = _bridge.DrawDistinction("ctx", reason);

        result.Should().Be(Form.Mark);
        _bridge.GetFormState("ctx").Should().Be(Form.Mark);
    }

    [Fact]
    public void DrawDistinction_RaisesDistinctionChanged()
    {
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        _bridge.DrawDistinction("test");

        capturedArgs.Should().NotBeNull();
        capturedArgs!.EventType.Should().Be(DistinctionEventType.DistinctionDrawn);
        capturedArgs.CurrentState.Should().Be(Form.Mark);
        capturedArgs.Context.Should().Be("test");
    }

    [Fact]
    public void DrawDistinction_PreviousStateIsVoidForNewContext()
    {
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        _bridge.DrawDistinction("new-context");

        capturedArgs!.PreviousState.Should().Be(Form.Void);
    }

    #endregion

    #region CrossDistinction

    [Fact]
    public void CrossDistinction_FromVoid_ReturnsMark()
    {
        var result = _bridge.CrossDistinction("test");

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void CrossDistinction_FromMark_ReturnsVoid()
    {
        _bridge.DrawDistinction("test");

        var result = _bridge.CrossDistinction("test");

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void CrossDistinction_DoubleCrossFromMark_IsCancelled()
    {
        _bridge.DrawDistinction("test");

        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        _bridge.CrossDistinction("test");

        capturedArgs!.EventType.Should().Be(DistinctionEventType.Cancelled);
    }

    [Fact]
    public void CrossDistinction_UpdatesContext()
    {
        _bridge.DrawDistinction("test");
        _bridge.CrossDistinction("test");

        _bridge.GetFormState("test").Should().Be(Form.Void);
    }

    #endregion

    #region CreateReEntry

    [Fact]
    public void CreateReEntry_ReturnsImaginary()
    {
        var result = _bridge.CreateReEntry("test");

        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void CreateReEntry_SetsContextState()
    {
        _bridge.CreateReEntry("test");

        _bridge.GetFormState("test").Should().Be(Form.Imaginary);
    }

    [Fact]
    public void CreateReEntry_RaisesDistinctionChanged()
    {
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        _bridge.CreateReEntry("test");

        capturedArgs.Should().NotBeNull();
        capturedArgs!.EventType.Should().Be(DistinctionEventType.ReEntryCreated);
        capturedArgs.CurrentState.Should().Be(Form.Imaginary);
    }

    #endregion

    #region EvaluateTruthValue

    [Fact]
    public void EvaluateTruthValue_FormAtomMark_ReturnsMark()
    {
        var result = _bridge.EvaluateTruthValue(FormAtom.Mark);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_FormAtomVoid_ReturnsVoid()
    {
        var result = _bridge.EvaluateTruthValue(FormAtom.Void);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_MarkSymbol_ReturnsMark()
    {
        var result = _bridge.EvaluateTruthValue(Atom.Sym("Mark"));

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_VoidSymbol_ReturnsVoid()
    {
        var result = _bridge.EvaluateTruthValue(Atom.Sym("Void"));

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_CrossExpression_Negates()
    {
        var expr = Atom.Expr(Atom.Sym("cross"), Atom.Sym("Void"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_CrossWithNoArgs_ReturnsMark()
    {
        var expr = Atom.Expr(Atom.Sym("cross"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_CallExpression_Evaluates()
    {
        var expr = Atom.Expr(Atom.Sym("call"), Atom.Sym("Void"), Atom.Sym("Void"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_CallWithInsufficientArgs_ReturnsVoid()
    {
        var expr = Atom.Expr(Atom.Sym("call"), Atom.Sym("Mark"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_AndExpression_Evaluates()
    {
        var expr = Atom.Expr(Atom.Sym("and"), Atom.Sym("Mark"), Atom.Sym("Void"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_OrExpression_Evaluates()
    {
        var expr = Atom.Expr(Atom.Sym("or"), Atom.Sym("Mark"), Atom.Sym("Void"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_NotExpression_Negates()
    {
        var expr = Atom.Expr(Atom.Sym("not"), Atom.Sym("Mark"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_NotWithNoArgs_ReturnsMark()
    {
        var expr = Atom.Expr(Atom.Sym("not"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_ReentryExpression_ReturnsImaginary()
    {
        var expr = Atom.Expr(Atom.Sym("reentry"), Atom.Sym("x"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithVoidCondition_ReturnsMarkVacuously()
    {
        var expr = Atom.Expr(Atom.Sym("implies"), Atom.Sym("Void"), Atom.Sym("Void"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithImaginaryCondition_ReturnsImaginary()
    {
        var expr = Atom.Expr(Atom.Sym("implies"), Atom.Sym("Imaginary"), Atom.Sym("Mark"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithMarkCondition_EvaluatesConclusion()
    {
        var expr = Atom.Expr(Atom.Sym("implies"), Atom.Sym("Mark"), Atom.Sym("Void"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithInsufficientArgs_ReturnsVoid()
    {
        var expr = Atom.Expr(Atom.Sym("implies"), Atom.Sym("Mark"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_RaisesTruthValueEvaluated()
    {
        TruthValueEventArgs? capturedArgs = null;
        _bridge.TruthValueEvaluated += (_, args) => capturedArgs = args;

        _bridge.EvaluateTruthValue(Atom.Sym("Mark"));

        capturedArgs.Should().NotBeNull();
        capturedArgs!.TruthValue.Should().Be(Form.Mark);
        capturedArgs.Expression.Should().Be(Atom.Sym("Mark"));
        capturedArgs.ReasoningTrace.Should().NotBeEmpty();
    }

    [Fact]
    public void EvaluateTruthValue_AndWithInsufficientArgs_ReturnsVoid()
    {
        var expr = Atom.Expr(Atom.Sym("and"), Atom.Sym("Mark"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_OrWithInsufficientArgs_ReturnsVoid()
    {
        var expr = Atom.Expr(Atom.Sym("or"), Atom.Sym("Mark"));

        var result = _bridge.EvaluateTruthValue(expr);

        result.Should().Be(Form.Void);
    }

    #endregion

    #region DistinctionGatedInference

    [Fact]
    public void DistinctionGatedInference_GuardNotMarked_ReturnsEmpty()
    {
        var query = Atom.Sym("test");

        var results = _bridge.DistinctionGatedInference("guard", query).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void DistinctionGatedInference_GuardIsMarked_ReturnsResults()
    {
        _bridge.DrawDistinction("guard");
        _space.Add(Atom.Sym("fact"));

        var results = _bridge.DistinctionGatedInference("guard", Atom.Sym("fact")).ToList();

        results.Should().NotBeEmpty();
    }

    #endregion

    #region MetaReason

    [Fact]
    public void MetaReason_Symbol_ReturnsMetaFacts()
    {
        var expr = Atom.Sym("test");

        var results = _bridge.MetaReason(expr).ToList();

        results.Should().NotBeEmpty();
        results.Should().Contain(a =>
            a is Expression e && e.Children.Count > 0 &&
            e.Children[0] is Symbol s && s.Name == "is-expression");
    }

    [Fact]
    public void MetaReason_Expression_ReturnsHeadAndArity()
    {
        var expr = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"), Atom.Sym("y"));

        var results = _bridge.MetaReason(expr).ToList();

        results.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void MetaReason_ExpressionWithVariables_ReportsHasVariablesTrue()
    {
        var expr = Atom.Expr(Atom.Sym("f"), Atom.Var("x"));

        var results = _bridge.MetaReason(expr).ToList();

        results.Should().Contain(a =>
            a is Expression e && e.Children.Count > 1 &&
            e.Children[0] is Symbol s && s.Name == "has-variables" &&
            e.Children[1] is Symbol v && v.Name == "True");
    }

    [Fact]
    public void MetaReason_RaisesMetaReasoningPerformed()
    {
        var events = new List<MetaReasoningEventArgs>();
        _bridge.MetaReasoningPerformed += (_, args) => events.Add(args);

        var results = _bridge.MetaReason(Atom.Sym("test")).ToList();

        events.Should().NotBeEmpty();
        events[0].Operation.Should().Be("analyze");
    }

    #endregion

    #region GetFormState / GetAllDistinctions / ClearDistinction

    [Fact]
    public void GetFormState_Unset_ReturnsVoid()
    {
        _bridge.GetFormState("nonexistent").Should().Be(Form.Void);
    }

    [Fact]
    public void GetAllDistinctions_ReturnsAllContexts()
    {
        _bridge.DrawDistinction("a");
        _bridge.DrawDistinction("b");

        var all = _bridge.GetAllDistinctions();

        all.Should().ContainKey("a");
        all.Should().ContainKey("b");
    }

    [Fact]
    public void ClearDistinction_RemovesContext()
    {
        _bridge.DrawDistinction("test");
        _bridge.ClearDistinction("test");

        _bridge.GetFormState("test").Should().Be(Form.Void);
    }

    [Fact]
    public void ClearDistinction_NonExistent_DoesNotThrow()
    {
        Action act = () => _bridge.ClearDistinction("nonexistent");

        act.Should().NotThrow();
    }

    #endregion

    #region Interpreter Property

    [Fact]
    public void Interpreter_IsAccessible()
    {
        _bridge.Interpreter.Should().NotBeNull();
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var space = new AtomSpace();
        var bridge = new FormMeTTaBridge(space);

        bridge.Dispose();
        Action act = () => bridge.Dispose();

        act.Should().NotThrow();
    }

    #endregion
}
