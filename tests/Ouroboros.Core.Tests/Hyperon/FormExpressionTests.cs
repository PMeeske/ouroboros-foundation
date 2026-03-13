using System.Collections.Immutable;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
public class FormExpressionTests
{
    [Fact]
    public void Cross_CreatesExpressionWithCrossOperator()
    {
        var sut = FormExpression.Cross(FormAtom.Mark);

        sut.Operator.Should().Be("cross");
        sut.Operands.Should().HaveCount(1);
    }

    [Fact]
    public void Call_CreatesExpressionWithCallOperator()
    {
        var sut = FormExpression.Call(FormAtom.Mark, FormAtom.Void);

        sut.Operator.Should().Be("call");
        sut.Operands.Should().HaveCount(2);
    }

    [Fact]
    public void ReEntry_CreatesExpressionWithReentryOperator()
    {
        var sut = FormExpression.ReEntry(FormAtom.Mark);

        sut.Operator.Should().Be("reentry");
        sut.Operands.Should().HaveCount(1);
    }

    [Fact]
    public void ToSExpr_FormatsCorrectly()
    {
        var sut = FormExpression.Cross(FormAtom.Mark);

        sut.ToSExpr().Should().Be("(cross ⌐)");
    }

    [Fact]
    public void ContainsVariables_NoVariables_ReturnsFalse()
    {
        var sut = FormExpression.Cross(FormAtom.Mark);

        sut.ContainsVariables().Should().BeFalse();
    }

    [Fact]
    public void ContainsVariables_WithVariable_ReturnsTrue()
    {
        var sut = FormExpression.Cross(Atom.Var("x"));

        sut.ContainsVariables().Should().BeTrue();
    }

    [Fact]
    public void Evaluate_Cross_NegatesInnerForm()
    {
        var sut = FormExpression.Cross(FormAtom.Mark);

        var result = sut.Evaluate();

        result.Form.Should().Be(LoF.Void);
    }

    [Fact]
    public void Evaluate_CrossVoid_ReturnsMark()
    {
        var sut = FormExpression.Cross(FormAtom.Void);

        var result = sut.Evaluate();

        result.Form.Should().Be(LoF.Mark);
    }

    [Fact]
    public void Evaluate_CrossEmpty_ReturnsMark()
    {
        // Cross with no operands returns Mark
        var sut = new FormExpression("cross", ImmutableList<Atom>.Empty);

        var result = sut.Evaluate();

        result.Form.Should().Be(LoF.Mark);
    }

    [Fact]
    public void Evaluate_CallWithTwoFormAtoms_ReturnsCallResult()
    {
        var sut = FormExpression.Call(FormAtom.Mark, FormAtom.Void);

        var result = sut.Evaluate();

        result.Should().NotBeNull();
    }

    [Fact]
    public void Evaluate_CallWithFewerThanTwoOperands_ReturnsVoid()
    {
        var sut = new FormExpression("call", ImmutableList.Create<Atom>(FormAtom.Mark));

        var result = sut.Evaluate();

        result.Form.Should().Be(LoF.Void);
    }

    [Fact]
    public void Evaluate_ReEntry_ReturnsImaginary()
    {
        var sut = FormExpression.ReEntry(FormAtom.Mark);

        var result = sut.Evaluate();

        result.Form.Should().Be(LoF.Imaginary);
    }

    [Fact]
    public void Evaluate_UnknownOperator_ReturnsVoid()
    {
        var sut = new FormExpression("unknown", ImmutableList.Create<Atom>(FormAtom.Mark));

        var result = sut.Evaluate();

        result.Form.Should().Be(LoF.Void);
    }

    [Fact]
    public void Evaluate_WithSymbolOperands_ResolvesNamedForms()
    {
        var sut = FormExpression.Cross(Atom.Sym("Mark"));

        var result = sut.Evaluate();

        result.Form.Should().Be(LoF.Void);
    }

    [Fact]
    public void Evaluate_WithVoidSymbol_ResolvesToVoid()
    {
        var sut = FormExpression.Cross(Atom.Sym("Void"));

        var result = sut.Evaluate();

        result.Form.Should().Be(LoF.Mark);
    }

    [Fact]
    public void Evaluate_NestedFormExpressions_EvaluatesRecursively()
    {
        // cross(cross(Mark)) should evaluate to Mark
        var inner = FormExpression.Cross(FormAtom.Mark);
        var sut = FormExpression.Cross(inner);

        var result = sut.Evaluate();

        result.Form.Should().Be(LoF.Mark);
    }

    [Fact]
    public void RecordEquality_SameOperatorAndOperands_AreEqual()
    {
        var a = FormExpression.Cross(FormAtom.Mark);
        var b = FormExpression.Cross(FormAtom.Mark);

        a.Should().Be(b);
    }
}
