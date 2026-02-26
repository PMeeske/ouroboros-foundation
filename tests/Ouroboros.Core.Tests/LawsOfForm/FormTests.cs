// <copyright file="FormTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the Form struct from Spencer-Brown's Laws of Form.
/// Validates three-valued logic: Mark, Void, Imaginary.
/// </summary>
[Trait("Category", "Unit")]
public class FormTests
{
    // --- State checks ---

    [Fact]
    public void Mark_IsMark_ReturnsTrue()
    {
        Form.Mark.IsMark().Should().BeTrue();
        Form.Mark.IsVoid().Should().BeFalse();
        Form.Mark.IsImaginary().Should().BeFalse();
    }

    [Fact]
    public void Void_IsVoid_ReturnsTrue()
    {
        Form.Void.IsVoid().Should().BeTrue();
        Form.Void.IsMark().Should().BeFalse();
        Form.Void.IsImaginary().Should().BeFalse();
    }

    [Fact]
    public void Imaginary_IsImaginary_ReturnsTrue()
    {
        Form.Imaginary.IsImaginary().Should().BeTrue();
        Form.Imaginary.IsMark().Should().BeFalse();
        Form.Imaginary.IsVoid().Should().BeFalse();
    }

    [Fact]
    public void IsCertain_MarkAndVoid_ReturnsTrue()
    {
        Form.Mark.IsCertain().Should().BeTrue();
        Form.Void.IsCertain().Should().BeTrue();
    }

    [Fact]
    public void IsCertain_Imaginary_ReturnsFalse()
    {
        Form.Imaginary.IsCertain().Should().BeFalse();
    }

    [Fact]
    public void Cross_ReturnsMark()
    {
        Form.Cross().IsMark().Should().BeTrue();
    }

    // --- Negation (Not / CrossForm) ---

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
        // Re-entry is self-negating: ~i = i
        Form.Imaginary.Not().Should().Be(Form.Imaginary);
    }

    [Fact]
    public void DoubleNegation_MarkReturnsToMark()
    {
        Form.Mark.Not().Not().Should().Be(Form.Mark);
    }

    [Fact]
    public void DoubleNegation_VoidReturnsToVoid()
    {
        Form.Void.Not().Not().Should().Be(Form.Void);
    }

    [Fact]
    public void CrossForm_EquivalentToNot()
    {
        Form.CrossForm(Form.Void).Should().Be(Form.Void.Not());
        Form.CrossForm(Form.Mark).Should().Be(Form.Mark.Not());
    }

    // --- Operator overloads ---

    [Fact]
    public void NegationOperator_EquivalentToNot()
    {
        (!Form.Mark).Should().Be(Form.Void);
        (!Form.Void).Should().Be(Form.Mark);
        (!Form.Imaginary).Should().Be(Form.Imaginary);
    }

    // --- Conjunction (And) ---

    [Fact]
    public void And_MarkAndMark_ReturnsMark()
    {
        (Form.Mark & Form.Mark).Should().Be(Form.Mark);
    }

    [Fact]
    public void And_MarkAndVoid_ReturnsVoid()
    {
        (Form.Mark & Form.Void).Should().Be(Form.Void);
    }

    [Fact]
    public void And_VoidAndVoid_ReturnsVoid()
    {
        (Form.Void & Form.Void).Should().Be(Form.Void);
    }

    [Fact]
    public void And_AnyWithImaginary_ReturnsImaginary()
    {
        (Form.Mark & Form.Imaginary).Should().Be(Form.Imaginary);
        (Form.Void & Form.Imaginary).Should().Be(Form.Imaginary);
        (Form.Imaginary & Form.Imaginary).Should().Be(Form.Imaginary);
    }

    // --- Disjunction (Or) ---

    [Fact]
    public void Or_MarkOrAnything_ReturnsMark()
    {
        (Form.Mark | Form.Void).Should().Be(Form.Mark);
        (Form.Mark | Form.Imaginary).Should().Be(Form.Mark);
        (Form.Mark | Form.Mark).Should().Be(Form.Mark);
    }

    [Fact]
    public void Or_VoidOrVoid_ReturnsVoid()
    {
        (Form.Void | Form.Void).Should().Be(Form.Void);
    }

    [Fact]
    public void Or_VoidOrImaginary_ReturnsImaginary()
    {
        (Form.Void | Form.Imaginary).Should().Be(Form.Imaginary);
    }

    // --- Calling ---

    [Fact]
    public void Calling_ReturnsItself()
    {
        Form.Mark.Calling().Should().Be(Form.Mark);
        Form.Void.Calling().Should().Be(Form.Void);
        Form.Imaginary.Calling().Should().Be(Form.Imaginary);
    }

    // --- Call ---

    [Fact]
    public void Call_ImaginaryDominates()
    {
        Form.Mark.Call(Form.Imaginary).Should().Be(Form.Imaginary);
        Form.Imaginary.Call(Form.Mark).Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Call_RealForms_UsesOrLogic()
    {
        Form.Mark.Call(Form.Void).Should().Be(Form.Mark);
        Form.Void.Call(Form.Mark).Should().Be(Form.Mark);
        Form.Void.Call(Form.Void).Should().Be(Form.Void);
    }

    // --- ToBool ---

    [Fact]
    public void ToBool_Mark_ReturnsTrue()
    {
        Form.Mark.ToBool().Should().BeTrue();
    }

    [Fact]
    public void ToBool_Void_ReturnsFalse()
    {
        Form.Void.ToBool().Should().BeFalse();
    }

    [Fact]
    public void ToBool_Imaginary_ReturnsNull()
    {
        Form.Imaginary.ToBool().Should().BeNull();
    }

    // --- Match ---

    [Fact]
    public void Match_Mark_ExecutesOnMark()
    {
        var result = Form.Mark.Match(
            onMark: () => "marked",
            onVoid: () => "void",
            onImaginary: () => "imaginary");

        result.Should().Be("marked");
    }

    [Fact]
    public void Match_Void_ExecutesOnVoid()
    {
        var result = Form.Void.Match(
            onMark: () => "marked",
            onVoid: () => "void",
            onImaginary: () => "imaginary");

        result.Should().Be("void");
    }

    [Fact]
    public void Match_Imaginary_ExecutesOnImaginary()
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
        string executed = "";
        Form.Void.Match(
            onMark: () => executed = "mark",
            onVoid: () => executed = "void",
            onImaginary: () => executed = "imaginary");

        executed.Should().Be("void");
    }

    // --- Equality ---

    [Fact]
    public void Equality_SameState_AreEqual()
    {
        Form.Mark.Should().Be(Form.Cross());
        Form.Void.Should().Be(Form.Void);
        Form.Imaginary.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Equality_DifferentState_AreNotEqual()
    {
        Form.Mark.Should().NotBe(Form.Void);
        Form.Mark.Should().NotBe(Form.Imaginary);
        Form.Void.Should().NotBe(Form.Imaginary);
    }

    [Fact]
    public void EqualityOperator_WorksCorrectly()
    {
        (Form.Mark == Form.Cross()).Should().BeTrue();
        (Form.Mark != Form.Void).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameState_SameHash()
    {
        Form.Mark.GetHashCode().Should().Be(Form.Cross().GetHashCode());
    }

    // --- ToString ---

    [Fact]
    public void ToString_ReturnsExpectedSymbols()
    {
        Form.Mark.ToString().Should().NotBeEmpty();
        Form.Void.ToString().Should().NotBeEmpty();
        Form.Imaginary.ToString().Should().NotBeEmpty();
    }

    // --- ReEntry / Imagine / Eval ---

    [Fact]
    public void ReEntry_ReturnsImaginary()
    {
        Form.ReEntry().Should().Be(Form.Imaginary);
        Form.ReEntry("self").Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Imagine_ReturnsImaginary()
    {
        Form.Imagine(Math.PI).Should().Be(Form.Imaginary);
    }

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

    // --- IsMarked alias ---

    [Fact]
    public void IsMarked_EquivalentToIsMark()
    {
        Form.Mark.IsMarked().Should().Be(Form.Mark.IsMark());
        Form.Void.IsMarked().Should().Be(Form.Void.IsMark());
    }
}
