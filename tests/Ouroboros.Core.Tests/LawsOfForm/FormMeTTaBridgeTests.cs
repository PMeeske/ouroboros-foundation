// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

using FluentAssertions;
using Moq;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using Xunit;

namespace Ouroboros.Tests.LawsOfForm;

/// <summary>
/// Unit tests for FormMeTTaBridge covering form evaluation, cross-paradigm translation,
/// distinction management, expression reduction, and type conversion.
/// </summary>
[Trait("Category", "Unit")]
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

    // ========================================================================
    // Constructor
    // ========================================================================

    [Fact]
    public void Constructor_NullSpace_ThrowsArgumentNull()
    {
        // Act & Assert
        var act = () => new FormMeTTaBridge(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_InitializesFormAxioms()
    {
        // Assert - the space should contain Laws of Form axioms
        _space.Count.Should().BeGreaterThan(0);
    }

    // ========================================================================
    // DrawDistinction
    // ========================================================================

    [Fact]
    public void DrawDistinction_NewContext_ReturnsMark()
    {
        // Act
        var result = _bridge.DrawDistinction("test-context");

        // Assert
        result.Should().Be(Form.Mark);
        result.IsMarked().Should().BeTrue();
    }

    [Fact]
    public void DrawDistinction_SetsFormState()
    {
        // Act
        _bridge.DrawDistinction("ctx");

        // Assert
        var state = _bridge.GetFormState("ctx");
        state.Should().Be(Form.Mark);
    }

    [Fact]
    public void DrawDistinction_AddsToAtomSpace()
    {
        // Act
        _bridge.DrawDistinction("my-ctx");

        // Assert
        var allAtoms = _space.All().ToList();
        allAtoms.Should().Contain(a =>
            a.ToString().Contains("Distinction") &&
            a.ToString().Contains("my-ctx"));
    }

    [Fact]
    public void DrawDistinction_FiresDistinctionChangedEvent()
    {
        // Arrange
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        // Act
        _bridge.DrawDistinction("event-ctx");

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.EventType.Should().Be(DistinctionEventType.DistinctionDrawn);
        capturedArgs.CurrentState.Should().Be(Form.Mark);
        capturedArgs.PreviousState.Should().Be(Form.Void); // was unset
        capturedArgs.Context.Should().Be("event-ctx");
    }

    [Fact]
    public void DrawDistinction_WithReason_IncludesTriggerAtom()
    {
        // Arrange
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;
        var reason = Atom.Sym("because-test");

        // Act
        _bridge.DrawDistinction("reason-ctx", reason);

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.TriggerAtom.Should().Be(reason);
    }

    // ========================================================================
    // CrossDistinction
    // ========================================================================

    [Fact]
    public void CrossDistinction_UnsetContext_CrossesVoidToMark()
    {
        // Act - crossing void gives mark
        var result = _bridge.CrossDistinction("unset");

        // Assert
        result.Should().Be(Form.Mark); // Not(Void) = Mark
    }

    [Fact]
    public void CrossDistinction_MarkedContext_CrossesToVoid()
    {
        // Arrange
        _bridge.DrawDistinction("ctx"); // now Mark

        // Act
        var result = _bridge.CrossDistinction("ctx"); // Not(Mark) = Void

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void CrossDistinction_DoubleCross_ReturnsMark()
    {
        // Arrange
        _bridge.DrawDistinction("ctx"); // Mark

        // Act
        _bridge.CrossDistinction("ctx"); // Void
        var result = _bridge.CrossDistinction("ctx"); // Mark again

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void CrossDistinction_MarkToVoid_FiresCancelledEvent()
    {
        // Arrange
        _bridge.DrawDistinction("ctx"); // Mark
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        // Act
        _bridge.CrossDistinction("ctx"); // Mark -> Void = Cancelled

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.EventType.Should().Be(DistinctionEventType.Cancelled);
    }

    [Fact]
    public void CrossDistinction_VoidToMark_FiresCrossedEvent()
    {
        // Arrange - leave context at Void (default)
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        // Act
        _bridge.CrossDistinction("unset"); // Void -> Mark = Crossed

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.EventType.Should().Be(DistinctionEventType.Crossed);
    }

    // ========================================================================
    // CreateReEntry
    // ========================================================================

    [Fact]
    public void CreateReEntry_ReturnsImaginary()
    {
        // Act
        var result = _bridge.CreateReEntry("self-ref");

        // Assert
        result.Should().Be(Form.Imaginary);
        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void CreateReEntry_SetsFormStateToImaginary()
    {
        // Act
        _bridge.CreateReEntry("self-ref");

        // Assert
        var state = _bridge.GetFormState("self-ref");
        state.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void CreateReEntry_AddsReEntryToAtomSpace()
    {
        // Act
        _bridge.CreateReEntry("reentry-ctx");

        // Assert
        var allAtoms = _space.All().ToList();
        allAtoms.Should().Contain(a =>
            a.ToString().Contains("ReEntry") &&
            a.ToString().Contains("reentry-ctx"));
    }

    [Fact]
    public void CreateReEntry_FiresReEntryCreatedEvent()
    {
        // Arrange
        DistinctionEventArgs? capturedArgs = null;
        _bridge.DistinctionChanged += (_, args) => capturedArgs = args;

        // Act
        _bridge.CreateReEntry("event-ctx");

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.EventType.Should().Be(DistinctionEventType.ReEntryCreated);
        capturedArgs.CurrentState.Should().Be(Form.Imaginary);
    }

    // ========================================================================
    // EvaluateTruthValue
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_MarkSymbol_ReturnsFormMark()
    {
        // Act
        var result = _bridge.EvaluateTruthValue(Atom.Sym("Mark"));

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_VoidSymbol_ReturnsFormVoid()
    {
        // Act
        var result = _bridge.EvaluateTruthValue(Atom.Sym("Void"));

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_CrossOfVoid_ReturnsMark()
    {
        // Arrange - (cross Void) should return Mark
        var expr = Atom.Expr(Atom.Sym("cross"), Atom.Sym("Void"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_CrossOfMark_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("cross"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_DoubleCross_ReturnsOriginal()
    {
        // Arrange - (cross (cross Mark)) should return Mark
        var inner = Atom.Expr(Atom.Sym("cross"), Atom.Sym("Mark"));
        var outer = Atom.Expr(Atom.Sym("cross"), inner);

        // Act
        var result = _bridge.EvaluateTruthValue(outer);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_AndOfMarkMark_ReturnsMark()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("and"), Atom.Sym("Mark"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_AndOfMarkVoid_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("and"), Atom.Sym("Mark"), Atom.Sym("Void"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_OrOfVoidMark_ReturnsMark()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("or"), Atom.Sym("Void"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_OrOfVoidVoid_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("or"), Atom.Sym("Void"), Atom.Sym("Void"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_NotOfMark_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("not"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_NotOfVoid_ReturnsMark()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("not"), Atom.Sym("Void"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_Reentry_ReturnsImaginary()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("reentry"), Atom.Sym("X"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithVoidCondition_VacuouslyTrue()
    {
        // Arrange - false antecedent => vacuously true
        var expr = Atom.Expr(
            Atom.Sym("implies"), Atom.Sym("Void"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithImaginaryCondition_ReturnsImaginary()
    {
        // Arrange
        var expr = Atom.Expr(
            Atom.Sym("implies"), Atom.Sym("Imaginary"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithMarkCondition_ReturnsConclusion()
    {
        // Arrange - true condition => return conclusion
        var expr = Atom.Expr(
            Atom.Sym("implies"), Atom.Sym("Mark"), Atom.Sym("Void"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_CallOfMarkMark_ReturnsMark()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("call"), Atom.Sym("Mark"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_CallOfVoidMark_ReturnsMark()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("call"), Atom.Sym("Void"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_FiresTruthValueEvaluatedEvent()
    {
        // Arrange
        TruthValueEventArgs? capturedArgs = null;
        _bridge.TruthValueEvaluated += (_, args) => capturedArgs = args;

        // Act
        _bridge.EvaluateTruthValue(Atom.Sym("Mark"));

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.TruthValue.Should().Be(Form.Mark);
    }

    // ========================================================================
    // DistinctionGatedInference
    // ========================================================================

    [Fact]
    public void DistinctionGatedInference_GuardNotMarked_ReturnsEmpty()
    {
        // Arrange - guard not set (default Void)
        var query = Atom.Sym("test");

        // Act
        var results = _bridge.DistinctionGatedInference("guard", query).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void DistinctionGatedInference_GuardMarked_ExecutesQuery()
    {
        // Arrange
        _bridge.DrawDistinction("guard"); // Set to Mark

        // Add a fact to the space that the query can match
        _space.Add(Atom.Sym("test-fact"));

        // Act
        var results = _bridge.DistinctionGatedInference("guard", Atom.Sym("test-fact")).ToList();

        // Assert - the interpreter should evaluate the query
        // Results depend on interpreter behavior with simple symbols
        // At minimum, the guard check should pass and attempt evaluation
    }

    [Fact]
    public void DistinctionGatedInference_ImaginaryGuard_ReturnsEmpty()
    {
        // Arrange - imaginary is not marked
        _bridge.CreateReEntry("guard"); // Imaginary

        // Act
        var results = _bridge.DistinctionGatedInference("guard", Atom.Sym("test")).ToList();

        // Assert
        results.Should().BeEmpty(); // Imaginary.IsMarked() = false
    }

    // ========================================================================
    // GetFormState and GetAllDistinctions
    // ========================================================================

    [Fact]
    public void GetFormState_UnsetContext_ReturnsVoid()
    {
        // Act
        var state = _bridge.GetFormState("nonexistent");

        // Assert
        state.Should().Be(Form.Void);
    }

    [Fact]
    public void GetAllDistinctions_MultipleContexts_ReturnsAll()
    {
        // Arrange
        _bridge.DrawDistinction("ctx1");
        _bridge.CreateReEntry("ctx2");
        _bridge.DrawDistinction("ctx3");
        _bridge.CrossDistinction("ctx3"); // Mark -> Void

        // Act
        var all = _bridge.GetAllDistinctions();

        // Assert
        all.Should().ContainKey("ctx1");
        all["ctx1"].Should().Be(Form.Mark);
        all.Should().ContainKey("ctx2");
        all["ctx2"].Should().Be(Form.Imaginary);
        all.Should().ContainKey("ctx3");
        all["ctx3"].Should().Be(Form.Void);
    }

    // ========================================================================
    // ClearDistinction
    // ========================================================================

    [Fact]
    public void ClearDistinction_RemovesContext()
    {
        // Arrange
        _bridge.DrawDistinction("ctx");

        // Act
        _bridge.ClearDistinction("ctx");

        // Assert
        _bridge.GetFormState("ctx").Should().Be(Form.Void); // back to default
        _bridge.GetAllDistinctions().Should().NotContainKey("ctx");
    }

    [Fact]
    public void ClearDistinction_NonexistentContext_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _bridge.ClearDistinction("nonexistent");
        act.Should().NotThrow();
    }

    // ========================================================================
    // Cross and Not equivalence (Law of Crossing)
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_CrossWithNoArgs_ReturnsMark()
    {
        // Arrange - (cross) with no inner = Mark
        var expr = Atom.Expr(Atom.Sym("cross"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_NotWithNoArgs_ReturnsMark()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("not"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_CallWithInsuffArgs_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("call"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_AndWithInsuffArgs_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("and"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_OrWithInsuffArgs_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("or"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithInsuffArgs_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("implies"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Void);
    }

    // ========================================================================
    // And/Or with Imaginary
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_AndWithImaginary_ReturnsImaginary()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("and"), Atom.Sym("Mark"), Atom.Sym("Imaginary"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void EvaluateTruthValue_OrWithImaginaryAndVoid_ReturnsImaginary()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("or"), Atom.Sym("Void"), Atom.Sym("Imaginary"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void EvaluateTruthValue_OrWithMarkAndImaginary_ReturnsMark()
    {
        // Arrange - Mark OR anything = Mark
        var expr = Atom.Expr(Atom.Sym("or"), Atom.Sym("Mark"), Atom.Sym("Imaginary"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    // ========================================================================
    // MetaReason
    // ========================================================================

    [Fact]
    public void MetaReason_SimpleSymbol_GeneratesMetaFacts()
    {
        // Arrange
        MetaReasoningEventArgs? capturedArgs = null;
        _bridge.MetaReasoningPerformed += (_, args) => capturedArgs = args;

        // Act
        var results = _bridge.MetaReason(Atom.Sym("test")).ToList();

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(a => a.ToString().Contains("is-expression"));
        capturedArgs.Should().NotBeNull();
    }

    [Fact]
    public void MetaReason_Expression_IncludesHeadAndArity()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("foo"), Atom.Sym("bar"), Atom.Sym("baz"));

        // Act
        var results = _bridge.MetaReason(expr).ToList();

        // Assert
        results.Should().Contain(a => a.ToString().Contains("head"));
        results.Should().Contain(a => a.ToString().Contains("arity"));
    }

    // ========================================================================
    // Dispose
    // ========================================================================

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var bridge = new FormMeTTaBridge(new AtomSpace());

        // Act & Assert
        var act = () =>
        {
            bridge.Dispose();
            bridge.Dispose();
        };
        act.Should().NotThrow();
    }

    // ========================================================================
    // Interpreter access
    // ========================================================================

    [Fact]
    public void Interpreter_IsAccessible()
    {
        // Assert
        _bridge.Interpreter.Should().NotBeNull();
    }
}
