using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class FormAtomTests
{
    [Fact]
    public void Constructor_SetsForm()
    {
        var atom = new FormAtom(Form.Mark);

        atom.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Mark_ReturnsFormAtomWithMarkForm()
    {
        var atom = FormAtom.Mark;

        atom.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Void_ReturnsFormAtomWithVoidForm()
    {
        var atom = FormAtom.Void;

        atom.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Imaginary_ReturnsFormAtomWithImaginaryForm()
    {
        var atom = FormAtom.Imaginary;

        atom.Form.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void ToSExpr_Mark_ReturnsCrossSymbol()
    {
        FormAtom.Mark.ToSExpr().Should().Be("⌐");
    }

    [Fact]
    public void ToSExpr_Void_ReturnsEmptySetSymbol()
    {
        FormAtom.Void.ToSExpr().Should().Be("∅");
    }

    [Fact]
    public void ToSExpr_Imaginary_ReturnsImaginarySymbol()
    {
        FormAtom.Imaginary.ToSExpr().Should().Be("ℑ");
    }

    [Fact]
    public void ContainsVariables_ReturnsFalse()
    {
        FormAtom.Mark.ContainsVariables().Should().BeFalse();
        FormAtom.Void.ContainsVariables().Should().BeFalse();
        FormAtom.Imaginary.ContainsVariables().Should().BeFalse();
    }

    [Fact]
    public void Cross_Mark_ReturnsVoid()
    {
        var result = FormAtom.Mark.Cross();

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Cross_Void_ReturnsMark()
    {
        var result = FormAtom.Void.Cross();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Cross_DoubleCrossing_CancelsOut()
    {
        var result = FormAtom.Mark.Cross().Cross();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Call_MarkWithMark_ReturnsMark()
    {
        var result = FormAtom.Mark.Call(FormAtom.Mark);

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Call_VoidWithVoid_ReturnsVoid()
    {
        var result = FormAtom.Void.Call(FormAtom.Void);

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void And_MarkWithMark_ReturnsMark()
    {
        var result = FormAtom.Mark.And(FormAtom.Mark);

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void And_MarkWithVoid_ReturnsVoid()
    {
        var result = FormAtom.Mark.And(FormAtom.Void);

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Or_MarkWithVoid_ReturnsMark()
    {
        var result = FormAtom.Mark.Or(FormAtom.Void);

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Or_VoidWithVoid_ReturnsVoid()
    {
        var result = FormAtom.Void.Or(FormAtom.Void);

        result.Form.Should().Be(Form.Void);
    }

    [Fact]
    public void Eval_ReturnsEvaluatedForm()
    {
        var result = FormAtom.Mark.Eval();

        result.Form.Should().Be(Form.Mark);
    }

    [Fact]
    public void Equality_SameForm_AreEqual()
    {
        var a = new FormAtom(Form.Mark);
        var b = new FormAtom(Form.Mark);

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentForms_AreNotEqual()
    {
        var mark = new FormAtom(Form.Mark);
        var voidAtom = new FormAtom(Form.Void);

        mark.Should().NotBe(voidAtom);
    }

    [Fact]
    public void IsAtom_FormAtomInheritsFromAtom()
    {
        Atom atom = FormAtom.Mark;

        atom.Should().BeOfType<FormAtom>();
    }
}
