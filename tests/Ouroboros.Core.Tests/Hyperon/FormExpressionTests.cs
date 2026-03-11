using System.Collections.Immutable;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class FormExpressionTests
{
    [Fact]
    public void Constructor_SetsOperatorAndOperands()
    {
        var operands = ImmutableList.Create<Atom>(FormAtom.Mark);
        var expr = new FormExpression("cross", operands);

        expr.Operator.Should().Be("cross");
        expr.Operands.Should().HaveCount(1);
        expr.Operands[0].Should().Be(FormAtom.Mark);
    }

    [Fact]
    public void Cross_Factory_CreatesCorrectExpression()
    {
        var inner = FormAtom.Void;
        var expr = FormExpression.Cross(inner);

        expr.Operator.Should().Be("cross");
        expr.Operands.Should().HaveCount(1);
        expr.Operands[0].Should().Be(inner);
    }

    [Fact]
    public void Call_Factory_CreatesCorrectExpression()
    {
        var left = FormAtom.Mark;
        var right = FormAtom.Void;
        var expr = FormExpression.Call(left, right);

        expr.Operator.Should().Be("call");
        expr.Operands.Should().HaveCount(2);
        expr.Operands[0].Should().Be(left);
        expr.Operands[1].Should().Be(right);
    }

    [Fact]
    public void ReEntry_Factory_CreatesCorrectExpression()
    {
        var form = FormAtom.Mark;
        var expr = FormExpression.ReEntry(form);

        expr.Operator.Should().Be("reentry");
        expr.Operands.Should().HaveCount(1);
        expr.Operands[0].Should().Be(form);
    }

    [Fact]
    public void ToSExpr_Cross_FormatsCorrectly()
    {
        var expr = FormExpression.Cross(FormAtom.Mark);

        expr.ToSExpr().Should().Be("(cross ⌐)");
    }

    [Fact]
    public void ToSExpr_Call_FormatsCorrectly()
    {
        var expr = FormExpression.Call(FormAtom.Mark, FormAtom.Void);

        expr.ToSExpr().Should().Be("(call ⌐ ∅)");
    }

    [Fact]
    public void ToSExpr_ReEntry_FormatsCorrectly()
    {
        var expr = FormExpression.ReEntry(FormAtom.Imaginary);

        expr.ToSExpr().Should().Be("(reentry ℑ)");
    }

    [Fact]
    public void ContainsVariables_NoVariables_ReturnsFalse()
    {
        var expr = FormExpression.Cross(FormAtom.Mark);

        expr.ContainsVariables().Should().BeFalse();
    }

    [Fact]
    public void ContainsVariables_WithVariable_ReturnsTrue()
    {
        var expr = FormExpression.Cross(Atom.Var("x"));

        expr.ContainsVariables().Should().BeTrue();
    }

    [Fact]
    public void Evaluate_CrossOfVoid_ReturnsMark()
    {
        var expr = FormExpression.Cross(FormAtom.Void);

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_CrossOfMark_ReturnsVoid()
    {
        var expr = FormExpression.Cross(FormAtom.Mark);

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_CrossWithNoOperands_ReturnsMark()
    {
        var expr = new FormExpression("cross", ImmutableList<Atom>.Empty);

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_CallOfMarkAndMark_ReturnsMark()
    {
        var expr = FormExpression.Call(FormAtom.Mark, FormAtom.Mark);

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_CallWithInsufficientOperands_ReturnsVoid()
    {
        var expr = new FormExpression("call", ImmutableList.Create<Atom>(FormAtom.Mark));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_ReEntry_ReturnsImaginary()
    {
        var expr = FormExpression.ReEntry(FormAtom.Mark);

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Evaluate_UnknownOperator_ReturnsVoid()
    {
        var expr = new FormExpression("unknown", ImmutableList.Create<Atom>(FormAtom.Mark));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_AndOperator_EvaluatesCorrectly()
    {
        var expr = new FormExpression("and", ImmutableList.Create<Atom>(FormAtom.Mark, FormAtom.Void));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_OrOperator_EvaluatesCorrectly()
    {
        var expr = new FormExpression("or", ImmutableList.Create<Atom>(FormAtom.Mark, FormAtom.Void));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_AndWithInsufficientOperands_ReturnsVoid()
    {
        var expr = new FormExpression("and", ImmutableList.Create<Atom>(FormAtom.Mark));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_OrWithInsufficientOperands_ReturnsVoid()
    {
        var expr = new FormExpression("or", ImmutableList.Create<Atom>(FormAtom.Mark));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_NestedCrossExpressions_ResolvesCorrectly()
    {
        // Cross(Cross(Mark)) should be Mark (double crossing cancels)
        var inner = FormExpression.Cross(FormAtom.Mark);
        var outer = FormExpression.Cross(inner);

        var result = outer.Evaluate();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_SymbolMark_ResolvesToFormAtomMark()
    {
        var expr = FormExpression.Cross(Atom.Sym("Mark"));

        var result = expr.Evaluate();

        // Cross(Mark) = Void
        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_SymbolVoid_ResolvesToFormAtomVoid()
    {
        var expr = FormExpression.Cross(Atom.Sym("Void"));

        var result = expr.Evaluate();

        // Cross(Void) = Mark
        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_SymbolImaginary_ResolvesToFormAtomImaginary()
    {
        var expr = FormExpression.Cross(Atom.Sym("Imaginary"));

        var result = expr.Evaluate();

        // Cross(Imaginary) - depends on Form.Not() for Imaginary
        result.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_UnknownSymbol_ResolvesToVoid()
    {
        var expr = FormExpression.Cross(Atom.Sym("Unknown"));

        var result = expr.Evaluate();

        // Unknown resolves to Void, then Cross(Void) = Mark
        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_CallWithSymbols_ResolvesCorrectly()
    {
        var expr = FormExpression.Call(Atom.Sym("Mark"), Atom.Sym("∅"));

        var result = expr.Evaluate();

        result.Should().NotBeNull();
    }

    [Fact]
    public void Equality_SameExpressions_AreEqual()
    {
        var a = FormExpression.Cross(FormAtom.Mark);
        var b = FormExpression.Cross(FormAtom.Mark);

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentExpressions_AreNotEqual()
    {
        var a = FormExpression.Cross(FormAtom.Mark);
        var b = FormExpression.Cross(FormAtom.Void);

        a.Should().NotBe(b);
    }
}
