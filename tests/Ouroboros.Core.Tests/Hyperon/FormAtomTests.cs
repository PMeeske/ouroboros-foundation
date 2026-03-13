using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
public class FormAtomTests
{
    [Fact]
    public void Mark_ReturnsFormAtomWithMarkForm()
    {
        var sut = FormAtom.Mark;

        sut.Form.Should().Be(LoF.Mark);
    }

    [Fact]
    public void Void_ReturnsFormAtomWithVoidForm()
    {
        var sut = FormAtom.Void;

        sut.Form.Should().Be(LoF.Void);
    }

    [Fact]
    public void Imaginary_ReturnsFormAtomWithImaginaryForm()
    {
        var sut = FormAtom.Imaginary;

        sut.Form.Should().Be(LoF.Imaginary);
    }

    [Fact]
    public void ToSExpr_Mark_ReturnsDistinctionSymbol()
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
    public void ContainsVariables_AlwaysFalse()
    {
        FormAtom.Mark.ContainsVariables().Should().BeFalse();
        FormAtom.Void.ContainsVariables().Should().BeFalse();
        FormAtom.Imaginary.ContainsVariables().Should().BeFalse();
    }

    [Fact]
    public void Cross_Mark_ReturnsVoid()
    {
        var result = FormAtom.Mark.Cross();

        result.Form.Should().Be(LoF.Void);
    }

    [Fact]
    public void Cross_Void_ReturnsMark()
    {
        var result = FormAtom.Void.Cross();

        result.Form.Should().Be(LoF.Mark);
    }

    [Fact]
    public void Call_MarkAndVoid_ReturnsExpectedResult()
    {
        var result = FormAtom.Mark.Call(FormAtom.Void);

        result.Should().NotBeNull();
    }

    [Fact]
    public void And_MarkAndMark_ReturnsMark()
    {
        var result = FormAtom.Mark.And(FormAtom.Mark);

        result.Form.Should().Be(LoF.Mark);
    }

    [Fact]
    public void And_MarkAndVoid_ReturnsVoid()
    {
        var result = FormAtom.Mark.And(FormAtom.Void);

        result.Form.Should().Be(LoF.Void);
    }

    [Fact]
    public void Or_MarkAndVoid_ReturnsMark()
    {
        var result = FormAtom.Mark.Or(FormAtom.Void);

        result.Form.Should().Be(LoF.Mark);
    }

    [Fact]
    public void Or_VoidAndVoid_ReturnsVoid()
    {
        var result = FormAtom.Void.Or(FormAtom.Void);

        result.Form.Should().Be(LoF.Void);
    }

    [Fact]
    public void Eval_ReturnsEvaluatedForm()
    {
        var result = FormAtom.Mark.Eval();

        result.Should().NotBeNull();
    }

    [Fact]
    public void RecordEquality_SameForm_AreEqual()
    {
        var a = new FormAtom(LoF.Mark);
        var b = new FormAtom(LoF.Mark);

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentForm_AreNotEqual()
    {
        FormAtom.Mark.Should().NotBe(FormAtom.Void);
    }
}
