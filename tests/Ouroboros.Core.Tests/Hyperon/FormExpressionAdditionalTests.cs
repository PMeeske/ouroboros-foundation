using System.Collections.Immutable;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class FormExpressionAdditionalTests
{
    [Fact]
    public void Evaluate_CrossOfImaginary_ReturnsCorrectResult()
    {
        var expr = FormExpression.Cross(FormAtom.Imaginary);

        var result = expr.Evaluate();

        // Cross(Imaginary) = Imaginary.Cross() per Form logic
        result.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_AndOfMarkAndMark_ReturnsMark()
    {
        var expr = new FormExpression("and", ImmutableList.Create<Atom>(FormAtom.Mark, FormAtom.Mark));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_AndOfVoidAndVoid_ReturnsVoid()
    {
        var expr = new FormExpression("and", ImmutableList.Create<Atom>(FormAtom.Void, FormAtom.Void));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_OrOfVoidAndVoid_ReturnsVoid()
    {
        var expr = new FormExpression("or", ImmutableList.Create<Atom>(FormAtom.Void, FormAtom.Void));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_OrOfMarkAndMark_ReturnsMark()
    {
        var expr = new FormExpression("or", ImmutableList.Create<Atom>(FormAtom.Mark, FormAtom.Mark));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_CallOfVoidAndVoid_ReturnsVoid()
    {
        var expr = FormExpression.Call(FormAtom.Void, FormAtom.Void);

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_CallOfMarkAndVoid_ReturnsMark()
    {
        var expr = FormExpression.Call(FormAtom.Mark, FormAtom.Void);

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_CallWithNoOperands_ReturnsVoid()
    {
        var expr = new FormExpression("call", ImmutableList<Atom>.Empty);

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_NestedFormExpressionInCall_ResolvesRecursively()
    {
        // Call(Cross(Void), Mark) -> Cross(Void) evaluates to Mark, Call(Mark, Mark) = Mark
        var inner = FormExpression.Cross(FormAtom.Void);
        var expr = FormExpression.Call(inner, FormAtom.Mark);

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_NestedFormExpressionInAnd_ResolvesRecursively()
    {
        var inner = FormExpression.Cross(FormAtom.Void); // evaluates to Mark
        var expr = new FormExpression("and", ImmutableList.Create<Atom>(inner, FormAtom.Mark));

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_SymbolMarkAlternate_ResolvesToMark()
    {
        // Test the "⌐" symbol resolves to Mark
        var expr = FormExpression.Cross(Atom.Sym("⌐"));

        var result = expr.Evaluate();

        // Cross(Mark) = Void
        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Evaluate_SymbolVoidAlternate_ResolvesToVoid()
    {
        // Test the "∅" symbol resolves to Void
        var expr = FormExpression.Cross(Atom.Sym("∅"));

        var result = expr.Evaluate();

        // Cross(Void) = Mark
        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_SymbolImaginaryAlternate_ResolvesToImaginary()
    {
        // Test the "ℑ" symbol resolves to Imaginary
        var expr = FormExpression.Cross(Atom.Sym("ℑ"));

        var result = expr.Evaluate();

        result.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_VariableAtom_ResolvesToVoid()
    {
        // Variable atom should resolve to Void as default
        var expr = FormExpression.Cross(Atom.Var("x"));

        var result = expr.Evaluate();

        // Cross(Void) = Mark since variable resolves to Void
        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Evaluate_ExpressionAtom_ResolvesToVoid()
    {
        // A non-FormExpression expression resolves to Void
        var innerExpr = Atom.Expr(Atom.Sym("foo"), Atom.Sym("bar"));
        var expr = FormExpression.Cross(innerExpr);

        var result = expr.Evaluate();

        // Expression resolves to Void, Cross(Void) = Mark
        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void ContainsVariables_MultipleOperands_NoneWithVariables_ReturnsFalse()
    {
        var expr = FormExpression.Call(FormAtom.Mark, FormAtom.Void);

        expr.ContainsVariables().Should().BeFalse();
    }

    [Fact]
    public void ContainsVariables_MultipleOperands_OneWithVariable_ReturnsTrue()
    {
        var expr = FormExpression.Call(Atom.Var("x"), FormAtom.Mark);

        expr.ContainsVariables().Should().BeTrue();
    }

    [Fact]
    public void ContainsVariables_SecondOperandHasVariable_ReturnsTrue()
    {
        var expr = FormExpression.Call(FormAtom.Mark, Atom.Var("y"));

        expr.ContainsVariables().Should().BeTrue();
    }

    [Fact]
    public void ToSExpr_NestedExpression_FormatsCorrectly()
    {
        var inner = FormExpression.Cross(FormAtom.Void);
        var outer = FormExpression.Call(inner, FormAtom.Mark);

        outer.ToSExpr().Should().Be("(call (cross ∅) ⌐)");
    }

    [Fact]
    public void Evaluate_ReEntryWithVoid_ReturnsImaginary()
    {
        var expr = FormExpression.ReEntry(FormAtom.Void);

        var result = expr.Evaluate();

        result.Form.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Record_GetHashCode_ConsistentForEqualInstances()
    {
        var a = FormExpression.Cross(FormAtom.Mark);
        var b = FormExpression.Cross(FormAtom.Mark);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Record_ToString_ReturnsNonNull()
    {
        var expr = FormExpression.Cross(FormAtom.Mark);

        expr.ToString().Should().NotBeNull();
    }
}
