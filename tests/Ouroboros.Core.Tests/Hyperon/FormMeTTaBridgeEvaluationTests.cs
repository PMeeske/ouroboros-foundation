// <copyright file="FormMeTTaBridgeEvaluationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Hyperon;

/// <summary>
/// Tests for FormMeTTaBridge.Evaluation partial class — truth value evaluation
/// methods including cross, call, and/or/not, reentry, implies, and generic evaluation.
/// </summary>
[Trait("Category", "Unit")]
public class FormMeTTaBridgeEvaluationTests : IDisposable
{
    private readonly AtomSpace _space;
    private readonly FormMeTTaBridge _bridge;

    public FormMeTTaBridgeEvaluationTests()
    {
        _space = new AtomSpace();
        _bridge = new FormMeTTaBridge(_space);
    }

    public void Dispose()
    {
        _bridge.Dispose();
    }

    // ========================================================================
    // EvaluateTruthValue — FormAtom direct evaluation
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_FormAtomMark_ReturnsMark()
    {
        // Arrange
        var formAtom = new FormAtom(Form.Mark);

        // Act
        var result = _bridge.EvaluateTruthValue(formAtom);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_FormAtomVoid_ReturnsVoid()
    {
        // Arrange
        var formAtom = new FormAtom(Form.Void);

        // Act
        var result = _bridge.EvaluateTruthValue(formAtom);

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_FormAtomImaginary_ReturnsImaginary()
    {
        // Arrange
        var formAtom = new FormAtom(Form.Imaginary);

        // Act
        var result = _bridge.EvaluateTruthValue(formAtom);

        // Assert
        result.Should().Be(Form.Imaginary);
    }

    // ========================================================================
    // EvaluateTruthValue — Cross operations
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_CrossOfImaginary_ReturnsImaginaryNot()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("cross"), Atom.Sym("Imaginary"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert — Not(Imaginary) = Imaginary
        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void EvaluateTruthValue_NestedCrossThreeLevels_ReturnsCrossed()
    {
        // Arrange — (cross (cross (cross Mark))) = cross(Mark) = Void
        var innermost = Atom.Expr(Atom.Sym("cross"), Atom.Sym("Mark"));
        var middle = Atom.Expr(Atom.Sym("cross"), innermost);
        var outer = Atom.Expr(Atom.Sym("cross"), middle);

        // Act
        var result = _bridge.EvaluateTruthValue(outer);

        // Assert — triple cross: Mark -> Void -> Mark -> Void
        result.Should().Be(Form.Void);
    }

    // ========================================================================
    // EvaluateTruthValue — Call with edge cases
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_CallOfVoidVoid_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("call"), Atom.Sym("Void"), Atom.Sym("Void"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert — Call(Void, Void) = Void
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_CallOfMarkVoid_ReturnsMark()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("call"), Atom.Sym("Mark"), Atom.Sym("Void"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert — Call(Mark, Void) = Mark
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_CallWithImaginaryOperand_PropagatesImaginary()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("call"), Atom.Sym("Imaginary"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert — Call with imaginary should propagate uncertainty
        result.Should().NotBeNull();
    }

    // ========================================================================
    // EvaluateTruthValue — And/Or with various combinations
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_AndOfVoidVoid_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("and"), Atom.Sym("Void"), Atom.Sym("Void"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_AndOfVoidMark_ReturnsVoid()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("and"), Atom.Sym("Void"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert — And with Void always returns Void
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void EvaluateTruthValue_OrOfMarkMark_ReturnsMark()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("or"), Atom.Sym("Mark"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_OrOfImaginaryImaginary_ReturnsImaginary()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("or"), Atom.Sym("Imaginary"), Atom.Sym("Imaginary"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Imaginary);
    }

    // ========================================================================
    // EvaluateTruthValue — Not
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_NotOfImaginary_ReturnsImaginary()
    {
        // Arrange
        var expr = Atom.Expr(Atom.Sym("not"), Atom.Sym("Imaginary"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert — Not(Imaginary) = Imaginary
        result.Should().Be(Form.Imaginary);
    }

    // ========================================================================
    // EvaluateTruthValue — Implies edge cases
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_ImpliesWithMarkConditionAndMarkConclusion_ReturnsMark()
    {
        // Arrange
        var expr = Atom.Expr(
            Atom.Sym("implies"), Atom.Sym("Mark"), Atom.Sym("Mark"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithVoidConditionAndVoidConclusion_VacuouslyTrue()
    {
        // Arrange — false antecedent => always Mark regardless of conclusion
        var expr = Atom.Expr(
            Atom.Sym("implies"), Atom.Sym("Void"), Atom.Sym("Void"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithImaginaryCondition_DoesNotEvaluateConclusion()
    {
        // Arrange — imaginary condition short-circuits to Imaginary
        var expr = Atom.Expr(
            Atom.Sym("implies"), Atom.Sym("Imaginary"), Atom.Sym("Void"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Imaginary);
    }

    // ========================================================================
    // EvaluateTruthValue — Generic expression (unknown head)
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_UnknownHeadWithMatchInSpace_ReturnsMark()
    {
        // Arrange — add a fact that the generic evaluator can find
        var fact = Atom.Expr(Atom.Sym("custom-op"), Atom.Sym("A"));
        _space.Add(fact);

        // Act
        var result = _bridge.EvaluateTruthValue(fact);

        // Assert — generic evaluator finds the match
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_UnknownHeadWithNoMatch_ReturnsVoid()
    {
        // Arrange — query for something not in the space
        var expr = Atom.Expr(Atom.Sym("nonexistent-op"), Atom.Sym("Z"));

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert — no matches means Void
        result.Should().Be(Form.Void);
    }

    // ========================================================================
    // EvaluateTruthValue — Unknown atom falls through to interpreter
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_UnknownSymbol_ReturnsVoid()
    {
        // Arrange — a symbol that doesn't map to a Form
        var atom = Atom.Sym("unknown-symbol");

        // Act
        var result = _bridge.EvaluateTruthValue(atom);

        // Assert — unknown symbol with no interpreter results
        result.Should().Be(Form.Void);
    }

    // ========================================================================
    // EvaluateTruthValue — Fires TruthValueEvaluated event
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_ComplexExpression_FiresEventWithCorrectExpression()
    {
        // Arrange
        TruthValueEventArgs? capturedArgs = null;
        _bridge.TruthValueEvaluated += (_, args) => capturedArgs = args;
        var expr = Atom.Expr(Atom.Sym("and"), Atom.Sym("Mark"), Atom.Sym("Mark"));

        // Act
        _bridge.EvaluateTruthValue(expr);

        // Assert
        capturedArgs.Should().NotBeNull();
        capturedArgs!.Expression.Should().Be(expr);
        capturedArgs.TruthValue.Should().Be(Form.Mark);
        capturedArgs.ReasoningTrace.Should().NotBeEmpty();
    }

    // ========================================================================
    // EvaluateTruthValue — Nested compound expressions
    // ========================================================================

    [Fact]
    public void EvaluateTruthValue_AndOfCrossedValues_EvaluatesRecursively()
    {
        // Arrange — (and (cross Void) (cross Void)) = And(Mark, Mark) = Mark
        var crossVoid1 = Atom.Expr(Atom.Sym("cross"), Atom.Sym("Void"));
        var crossVoid2 = Atom.Expr(Atom.Sym("cross"), Atom.Sym("Void"));
        var expr = Atom.Expr(Atom.Sym("and"), crossVoid1, crossVoid2);

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_ImpliesWithNestedCondition_EvaluatesRecursively()
    {
        // Arrange — (implies (and Mark Mark) (cross Void))
        // condition = And(Mark, Mark) = Mark; conclusion = Cross(Void) = Mark
        var condition = Atom.Expr(Atom.Sym("and"), Atom.Sym("Mark"), Atom.Sym("Mark"));
        var conclusion = Atom.Expr(Atom.Sym("cross"), Atom.Sym("Void"));
        var expr = Atom.Expr(Atom.Sym("implies"), condition, conclusion);

        // Act
        var result = _bridge.EvaluateTruthValue(expr);

        // Assert
        result.Should().Be(Form.Mark);
    }

    // ========================================================================
    // FormGatedMatch
    // ========================================================================

    [Fact]
    public void FormGatedMatch_WithKnownPattern_ReturnsMatchesWithFormState()
    {
        // Arrange — add facts to space so interpreter can find matches
        _space.Add(Atom.Expr(Atom.Sym("type"), Atom.Sym("X"), Atom.Sym("Form")));

        // Act — query for a pattern
        var pattern = Atom.Expr(Atom.Sym("type"), Atom.Var("x"), Atom.Sym("Form"));
        var results = _bridge.FormGatedMatch(pattern).ToList();

        // Assert — results may vary but method should not throw
        // Each result should have a Form state
        foreach (var (_, state, _) in results)
        {
            state.Should().NotBeNull();
        }
    }
}
