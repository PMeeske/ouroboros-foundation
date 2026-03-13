using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class FormTests
{
    // --- Construction ---

    [Fact]
    public void Mark_IsMark()
    {
        LoF.Mark.IsMark().Should().BeTrue();
        LoF.Mark.IsVoid().Should().BeFalse();
        LoF.Mark.IsImaginary().Should().BeFalse();
    }

    [Fact]
    public void Void_IsVoid()
    {
        LoF.Void.IsVoid().Should().BeTrue();
        LoF.Void.IsMark().Should().BeFalse();
        LoF.Void.IsImaginary().Should().BeFalse();
    }

    [Fact]
    public void Imaginary_IsImaginary()
    {
        LoF.Imaginary.IsImaginary().Should().BeTrue();
        LoF.Imaginary.IsMark().Should().BeFalse();
        LoF.Imaginary.IsVoid().Should().BeFalse();
    }

    [Fact]
    public void Cross_ReturnsMark()
    {
        LoF.Cross().IsMark().Should().BeTrue();
    }

    // --- IsCertain / IsMarked ---

    [Fact]
    public void IsCertain_Mark_ReturnsTrue()
    {
        LoF.Mark.IsCertain().Should().BeTrue();
    }

    [Fact]
    public void IsCertain_Void_ReturnsTrue()
    {
        LoF.Void.IsCertain().Should().BeTrue();
    }

    [Fact]
    public void IsCertain_Imaginary_ReturnsFalse()
    {
        LoF.Imaginary.IsCertain().Should().BeFalse();
    }

    [Fact]
    public void IsMarked_IsAliasForIsMark()
    {
        LoF.Mark.IsMarked().Should().BeTrue();
        LoF.Void.IsMarked().Should().BeFalse();
    }

    // --- Not (Negation) ---

    [Fact]
    public void Not_Mark_ReturnsVoid()
    {
        LoF.Mark.Not().Should().Be(LoF.Void);
    }

    [Fact]
    public void Not_Void_ReturnsMark()
    {
        LoF.Void.Not().Should().Be(LoF.Mark);
    }

    [Fact]
    public void Not_Imaginary_ReturnsImaginary()
    {
        LoF.Imaginary.Not().Should().Be(LoF.Imaginary);
    }

    [Fact]
    public void DoubleNegation_Cancels()
    {
        LoF.Mark.Not().Not().Should().Be(LoF.Mark);
        LoF.Void.Not().Not().Should().Be(LoF.Void);
    }

    [Fact]
    public void NegationOperator_Works()
    {
        (!LoF.Mark).Should().Be(LoF.Void);
        (!LoF.Void).Should().Be(LoF.Mark);
        (!LoF.Imaginary).Should().Be(LoF.Imaginary);
    }

    // --- CrossForm ---

    [Fact]
    public void CrossForm_AppliesNegation()
    {
        LoF.CrossForm(LoF.Void).Should().Be(LoF.Mark);
        LoF.CrossForm(LoF.Mark).Should().Be(LoF.Void);
    }

    [Fact]
    public void CrossForm_DoubleCross_Cancels()
    {
        LoF.CrossForm(LoF.CrossForm(LoF.Void)).Should().Be(LoF.Void);
    }

    // --- And (Conjunction) ---

    [Fact]
    public void And_MarkAndMark_ReturnsMark()
    {
        LoF.Mark.And(LoF.Mark).IsMark().Should().BeTrue();
    }

    [Fact]
    public void And_MarkAndVoid_ReturnsVoid()
    {
        LoF.Mark.And(LoF.Void).IsVoid().Should().BeTrue();
    }

    [Fact]
    public void And_VoidAndMark_ReturnsVoid()
    {
        LoF.Void.And(LoF.Mark).IsVoid().Should().BeTrue();
    }

    [Fact]
    public void And_ImaginaryDominates()
    {
        LoF.Mark.And(LoF.Imaginary).IsImaginary().Should().BeTrue();
        LoF.Imaginary.And(LoF.Void).IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void AndOperator_Works()
    {
        (LoF.Mark & LoF.Mark).IsMark().Should().BeTrue();
        (LoF.Mark & LoF.Void).IsVoid().Should().BeTrue();
    }

    // --- Or (Disjunction) ---

    [Fact]
    public void Or_MarkOrVoid_ReturnsMark()
    {
        LoF.Mark.Or(LoF.Void).IsMark().Should().BeTrue();
    }

    [Fact]
    public void Or_VoidOrVoid_ReturnsVoid()
    {
        LoF.Void.Or(LoF.Void).IsVoid().Should().BeTrue();
    }

    [Fact]
    public void Or_VoidOrImaginary_ReturnsImaginary()
    {
        LoF.Void.Or(LoF.Imaginary).IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Or_MarkOrImaginary_ReturnsMark()
    {
        // Mark dominates over Imaginary in Or
        LoF.Mark.Or(LoF.Imaginary).IsMark().Should().BeTrue();
    }

    [Fact]
    public void OrOperator_Works()
    {
        (LoF.Mark | LoF.Void).IsMark().Should().BeTrue();
        (LoF.Void | LoF.Void).IsVoid().Should().BeTrue();
    }

    // --- ToBool ---

    [Fact]
    public void ToBool_Mark_ReturnsTrue()
    {
        LoF.Mark.ToBool().Should().Be(true);
    }

    [Fact]
    public void ToBool_Void_ReturnsFalse()
    {
        LoF.Void.ToBool().Should().Be(false);
    }

    [Fact]
    public void ToBool_Imaginary_ReturnsNull()
    {
        LoF.Imaginary.ToBool().Should().BeNull();
    }

    // --- Match (pattern matching) ---

    [Fact]
    public void Match_Mark_CallsOnMark()
    {
        var result = LoF.Mark.Match(
            onMark: () => "marked",
            onVoid: () => "void",
            onImaginary: () => "imaginary");

        result.Should().Be("marked");
    }

    [Fact]
    public void Match_Void_CallsOnVoid()
    {
        var result = LoF.Void.Match(
            onMark: () => "marked",
            onVoid: () => "void",
            onImaginary: () => "imaginary");

        result.Should().Be("void");
    }

    [Fact]
    public void Match_Imaginary_CallsOnImaginary()
    {
        var result = LoF.Imaginary.Match(
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
        LoF.Mark.Match(
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
        LoF.Mark.Calling().Should().Be(LoF.Mark);
        LoF.Void.Calling().Should().Be(LoF.Void);
        LoF.Imaginary.Calling().Should().Be(LoF.Imaginary);
    }

    // --- Eval ---

    [Fact]
    public void Eval_IsIdempotent()
    {
        LoF.Mark.Eval().Should().Be(LoF.Mark);
        LoF.Void.Eval().Should().Be(LoF.Void);
        LoF.Imaginary.Eval().Should().Be(LoF.Imaginary);
    }

    // --- EvalToRecord ---

    [Fact]
    public void EvalToRecord_Mark_ReturnsMarkForm()
    {
        LoF.Mark.EvalToRecord().Should().BeOfType<LoF.MarkForm>();
    }

    [Fact]
    public void EvalToRecord_Void_ReturnsVoidForm()
    {
        LoF.Void.EvalToRecord().Should().BeOfType<LoF.VoidForm>();
    }

    [Fact]
    public void EvalToRecord_Imaginary_ReturnsImaginaryForm()
    {
        LoF.Imaginary.EvalToRecord().Should().BeOfType<LoF.ImaginaryForm>();
    }

    // --- Call ---

    [Fact]
    public void Call_ImaginaryDominates()
    {
        LoF.Mark.Call(LoF.Imaginary).IsImaginary().Should().BeTrue();
        LoF.Imaginary.Call(LoF.Void).IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Call_RealForms_UsesOrLogic()
    {
        LoF.Mark.Call(LoF.Void).IsMark().Should().BeTrue();
        LoF.Void.Call(LoF.Void).IsVoid().Should().BeTrue();
    }

    // --- ReEntry / Imagine ---

    [Fact]
    public void ReEntry_ReturnsImaginary()
    {
        LoF.ReEntry().IsImaginary().Should().BeTrue();
        LoF.ReEntry("self").IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Imagine_ReturnsImaginary()
    {
        LoF.Imagine(3.14).IsImaginary().Should().BeTrue();
    }

    // --- Equality ---

    [Fact]
    public void Equality_SameStates_AreEqual()
    {
        (LoF.Mark == LoF.Cross()).Should().BeTrue();
        (LoF.Void == LoF.Void).Should().BeTrue();
        (LoF.Imaginary == LoF.Imaginary).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentStates_AreNotEqual()
    {
        (LoF.Mark != LoF.Void).Should().BeTrue();
        (LoF.Mark != LoF.Imaginary).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        object boxed = LoF.Mark;
        LoF.Mark.Equals(boxed).Should().BeTrue();
        LoF.Mark.Equals("not a form").Should().BeFalse();
        LoF.Mark.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_EqualForms_SameHash()
    {
        LoF.Mark.GetHashCode().Should().Be(LoF.Cross().GetHashCode());
    }

    // --- ToString ---

    [Fact]
    public void ToString_ReturnsSymbols()
    {
        LoF.Mark.ToString().Should().NotBeNullOrEmpty();
        LoF.Void.ToString().Should().NotBeNullOrEmpty();
        LoF.Imaginary.ToString().Should().NotBeNullOrEmpty();
    }
}
