using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class FormTests
{
    // --- Construction ---

    [Fact]
    public void Mark_IsMark()
    {
        Form.Mark.IsMark().Should().BeTrue();
        Form.Mark.IsVoid().Should().BeFalse();
        Form.Mark.IsImaginary().Should().BeFalse();
    }

    [Fact]
    public void Void_IsVoid()
    {
        Form.Void.IsVoid().Should().BeTrue();
        Form.Void.IsMark().Should().BeFalse();
        Form.Void.IsImaginary().Should().BeFalse();
    }

    [Fact]
    public void Imaginary_IsImaginary()
    {
        Form.Imaginary.IsImaginary().Should().BeTrue();
        Form.Imaginary.IsMark().Should().BeFalse();
        Form.Imaginary.IsVoid().Should().BeFalse();
    }

    [Fact]
    public void Cross_ReturnsMark()
    {
        Form.Cross().IsMark().Should().BeTrue();
    }

    // --- IsCertain / IsMarked ---

    [Fact]
    public void IsCertain_Mark_ReturnsTrue()
    {
        Form.Mark.IsCertain().Should().BeTrue();
    }

    [Fact]
    public void IsCertain_Void_ReturnsTrue()
    {
        Form.Void.IsCertain().Should().BeTrue();
    }

    [Fact]
    public void IsCertain_Imaginary_ReturnsFalse()
    {
        Form.Imaginary.IsCertain().Should().BeFalse();
    }

    [Fact]
    public void IsMarked_IsAliasForIsMark()
    {
        Form.Mark.IsMarked().Should().BeTrue();
        Form.Void.IsMarked().Should().BeFalse();
    }

    // --- Not (Negation) ---

    [Fact]
    public void Not_Mark_ReturnsVoid()
    {
        Form.Mark.Not().Should().Be(Form.Void);
    }

    [Fact]
    public void Not_Void_ReturnsMark()
    {
        Form.Void.Not().Should().Be(Form.Mark);
    }

    [Fact]
    public void Not_Imaginary_ReturnsImaginary()
    {
        Form.Imaginary.Not().Should().Be(Form.Imaginary);
    }

    [Fact]
    public void DoubleNegation_Cancels()
    {
        Form.Mark.Not().Not().Should().Be(Form.Mark);
        Form.Void.Not().Not().Should().Be(Form.Void);
    }

    [Fact]
    public void NegationOperator_Works()
    {
        (!Form.Mark).Should().Be(Form.Void);
        (!Form.Void).Should().Be(Form.Mark);
        (!Form.Imaginary).Should().Be(Form.Imaginary);
    }

    // --- CrossForm ---

    [Fact]
    public void CrossForm_AppliesNegation()
    {
        Form.CrossForm(Form.Void).Should().Be(Form.Mark);
        Form.CrossForm(Form.Mark).Should().Be(Form.Void);
    }

    [Fact]
    public void CrossForm_DoubleCross_Cancels()
    {
        Form.CrossForm(Form.CrossForm(Form.Void)).Should().Be(Form.Void);
    }

    // --- And (Conjunction) ---

    [Fact]
    public void And_MarkAndMark_ReturnsMark()
    {
        Form.Mark.And(Form.Mark).IsMark().Should().BeTrue();
    }

    [Fact]
    public void And_MarkAndVoid_ReturnsVoid()
    {
        Form.Mark.And(Form.Void).IsVoid().Should().BeTrue();
    }

    [Fact]
    public void And_VoidAndMark_ReturnsVoid()
    {
        Form.Void.And(Form.Mark).IsVoid().Should().BeTrue();
    }

    [Fact]
    public void And_ImaginaryDominates()
    {
        Form.Mark.And(Form.Imaginary).IsImaginary().Should().BeTrue();
        Form.Imaginary.And(Form.Void).IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void AndOperator_Works()
    {
        (Form.Mark & Form.Mark).IsMark().Should().BeTrue();
        (Form.Mark & Form.Void).IsVoid().Should().BeTrue();
    }

    // --- Or (Disjunction) ---

    [Fact]
    public void Or_MarkOrVoid_ReturnsMark()
    {
        Form.Mark.Or(Form.Void).IsMark().Should().BeTrue();
    }

    [Fact]
    public void Or_VoidOrVoid_ReturnsVoid()
    {
        Form.Void.Or(Form.Void).IsVoid().Should().BeTrue();
    }

    [Fact]
    public void Or_VoidOrImaginary_ReturnsImaginary()
    {
        Form.Void.Or(Form.Imaginary).IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Or_MarkOrImaginary_ReturnsMark()
    {
        // Mark dominates over Imaginary in Or
        Form.Mark.Or(Form.Imaginary).IsMark().Should().BeTrue();
    }

    [Fact]
    public void OrOperator_Works()
    {
        (Form.Mark | Form.Void).IsMark().Should().BeTrue();
        (Form.Void | Form.Void).IsVoid().Should().BeTrue();
    }

    // --- ToBool ---

    [Fact]
    public void ToBool_Mark_ReturnsTrue()
    {
        Form.Mark.ToBool().Should().Be(true);
    }

    [Fact]
    public void ToBool_Void_ReturnsFalse()
    {
        Form.Void.ToBool().Should().Be(false);
    }

    [Fact]
    public void ToBool_Imaginary_ReturnsNull()
    {
        Form.Imaginary.ToBool().Should().BeNull();
    }

    // --- Match (pattern matching) ---

    [Fact]
    public void Match_Mark_CallsOnMark()
    {
        var result = Form.Mark.Match(
            onMark: () => "marked",
            onVoid: () => "void",
            onImaginary: () => "imaginary");

        result.Should().Be("marked");
    }

    [Fact]
    public void Match_Void_CallsOnVoid()
    {
        var result = Form.Void.Match(
            onMark: () => "marked",
            onVoid: () => "void",
            onImaginary: () => "imaginary");

        result.Should().Be("void");
    }

    [Fact]
    public void Match_Imaginary_CallsOnImaginary()
    {
        var result = Form.Imaginary.Match(
            onMark: () => "marked",
            onVoid: () => "void",
            onImaginary: () => "imaginary");

        result.Should().Be("imaginary");
    }

    [Fact]
    public void MatchAction_ExecutesCorrectBranch()
    {
        // Arrange
        string captured = "";

        // Act
        Form.Mark.Match(
            onMark: () => captured = "mark",
            onVoid: () => captured = "void",
            onImaginary: () => captured = "imaginary");

        // Assert
        captured.Should().Be("mark");
    }

    // --- Calling ---

    [Fact]
    public void Calling_ReturnsItself()
    {
        Form.Mark.Calling().Should().Be(Form.Mark);
        Form.Void.Calling().Should().Be(Form.Void);
        Form.Imaginary.Calling().Should().Be(Form.Imaginary);
    }

    // --- Eval ---

    [Fact]
    public void Eval_IsIdempotent()
    {
        Form.Mark.Eval().Should().Be(Form.Mark);
        Form.Void.Eval().Should().Be(Form.Void);
        Form.Imaginary.Eval().Should().Be(Form.Imaginary);
    }

    // --- EvalToRecord ---

    [Fact]
    public void EvalToRecord_Mark_ReturnsMarkForm()
    {
        Form.Mark.EvalToRecord().Should().BeOfType<Form.MarkForm>();
    }

    [Fact]
    public void EvalToRecord_Void_ReturnsVoidForm()
    {
        Form.Void.EvalToRecord().Should().BeOfType<Form.VoidForm>();
    }

    [Fact]
    public void EvalToRecord_Imaginary_ReturnsImaginaryForm()
    {
        Form.Imaginary.EvalToRecord().Should().BeOfType<Form.ImaginaryForm>();
    }

    // --- Call ---

    [Fact]
    public void Call_ImaginaryDominates()
    {
        Form.Mark.Call(Form.Imaginary).IsImaginary().Should().BeTrue();
        Form.Imaginary.Call(Form.Void).IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Call_RealForms_UsesOrLogic()
    {
        Form.Mark.Call(Form.Void).IsMark().Should().BeTrue();
        Form.Void.Call(Form.Void).IsVoid().Should().BeTrue();
    }

    // --- ReEntry / Imagine ---

    [Fact]
    public void ReEntry_ReturnsImaginary()
    {
        Form.ReEntry().IsImaginary().Should().BeTrue();
        Form.ReEntry("self").IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Imagine_ReturnsImaginary()
    {
        Form.Imagine(3.14).IsImaginary().Should().BeTrue();
    }

    // --- Equality ---

    [Fact]
    public void Equality_SameStates_AreEqual()
    {
        (Form.Mark == Form.Cross()).Should().BeTrue();
        (Form.Void == Form.Void).Should().BeTrue();
        (Form.Imaginary == Form.Imaginary).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentStates_AreNotEqual()
    {
        (Form.Mark != Form.Void).Should().BeTrue();
        (Form.Mark != Form.Imaginary).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        object boxed = Form.Mark;
        Form.Mark.Equals(boxed).Should().BeTrue();
        Form.Mark.Equals("not a form").Should().BeFalse();
        Form.Mark.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_EqualForms_SameHash()
    {
        Form.Mark.GetHashCode().Should().Be(Form.Cross().GetHashCode());
    }

    // --- ToString ---

    [Fact]
    public void ToString_ReturnsSymbols()
    {
        Form.Mark.ToString().Should().NotBeNullOrEmpty();
        Form.Void.ToString().Should().NotBeNullOrEmpty();
        Form.Imaginary.ToString().Should().NotBeNullOrEmpty();
    }
}
