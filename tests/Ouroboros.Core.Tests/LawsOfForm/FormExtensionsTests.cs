// <copyright file="FormExtensionsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for FormExtensions covering conversions, logical combinators, and superposition.
/// </summary>
[Trait("Category", "Unit")]
public class FormExtensionsTests
{
    // --- Boolean conversions ---

    [Fact]
    public void ToForm_True_ReturnsMark()
    {
        true.ToForm().Should().Be(Form.Mark);
    }

    [Fact]
    public void ToForm_False_ReturnsVoid()
    {
        false.ToForm().Should().Be(Form.Void);
    }

    [Fact]
    public void ToForm_NullableBoolTrue_ReturnsMark()
    {
        ((bool?)true).ToForm().Should().Be(Form.Mark);
    }

    [Fact]
    public void ToForm_NullableBoolFalse_ReturnsVoid()
    {
        ((bool?)false).ToForm().Should().Be(Form.Void);
    }

    [Fact]
    public void ToForm_NullableBoolNull_ReturnsImaginary()
    {
        ((bool?)null).ToForm().Should().Be(Form.Imaginary);
    }

    // --- Confidence conversions ---

    [Fact]
    public void ToForm_HighConfidence_ReturnsMark()
    {
        0.9.ToForm().Should().Be(Form.Mark);
    }

    [Fact]
    public void ToForm_LowConfidence_ReturnsVoid()
    {
        0.1.ToForm().Should().Be(Form.Void);
    }

    [Fact]
    public void ToForm_MediumConfidence_ReturnsImaginary()
    {
        0.5.ToForm().Should().Be(Form.Imaginary);
    }

    [Fact]
    public void ToForm_AtHighThreshold_ReturnsMark()
    {
        0.8.ToForm(highThreshold: 0.8).Should().Be(Form.Mark);
    }

    [Fact]
    public void ToForm_AtLowThreshold_ReturnsVoid()
    {
        0.3.ToForm(lowThreshold: 0.3).Should().Be(Form.Void);
    }

    [Fact]
    public void ToForm_CustomThresholds_WorkCorrectly()
    {
        0.6.ToForm(highThreshold: 0.5, lowThreshold: 0.2).Should().Be(Form.Mark);
        0.1.ToForm(highThreshold: 0.5, lowThreshold: 0.2).Should().Be(Form.Void);
        0.35.ToForm(highThreshold: 0.5, lowThreshold: 0.2).Should().Be(Form.Imaginary);
    }

    // --- Nullable struct conversion ---

    [Fact]
    public void ToForm_NullableIntWithValue_ReturnsMark()
    {
        ((int?)42).ToForm().Should().Be(Form.Mark);
    }

    [Fact]
    public void ToForm_NullableIntNull_ReturnsVoid()
    {
        ((int?)null).ToForm().Should().Be(Form.Void);
    }

    // --- ToFormRef ---

    [Fact]
    public void ToFormRef_NonNull_ReturnsMark()
    {
        "hello".ToFormRef().Should().Be(Form.Mark);
    }

    [Fact]
    public void ToFormRef_Null_ReturnsVoid()
    {
        ((string?)null).ToFormRef().Should().Be(Form.Void);
    }

    // --- All (conjunction) ---

    [Fact]
    public void All_EmptyArray_ReturnsMark()
    {
        FormExtensions.All().Should().Be(Form.Mark);
    }

    [Fact]
    public void All_AllMark_ReturnsMark()
    {
        FormExtensions.All(Form.Mark, Form.Mark, Form.Mark).Should().Be(Form.Mark);
    }

    [Fact]
    public void All_AnyVoid_ReturnsVoid()
    {
        FormExtensions.All(Form.Mark, Form.Void, Form.Mark).Should().Be(Form.Void);
    }

    [Fact]
    public void All_AnyImaginary_ReturnsImaginary()
    {
        FormExtensions.All(Form.Mark, Form.Imaginary).Should().Be(Form.Imaginary);
    }

    // --- Any (disjunction) ---

    [Fact]
    public void Any_EmptyArray_ReturnsVoid()
    {
        FormExtensions.Any().Should().Be(Form.Void);
    }

    [Fact]
    public void Any_AnyMark_ReturnsMark()
    {
        FormExtensions.Any(Form.Void, Form.Mark, Form.Void).Should().Be(Form.Mark);
    }

    [Fact]
    public void Any_AllVoid_ReturnsVoid()
    {
        FormExtensions.Any(Form.Void, Form.Void).Should().Be(Form.Void);
    }

    [Fact]
    public void Any_ImaginaryWithoutMark_ReturnsImaginary()
    {
        FormExtensions.Any(Form.Void, Form.Imaginary).Should().Be(Form.Imaginary);
    }

    // --- Superposition ---

    [Fact]
    public void Superposition_Empty_ReturnsVoid()
    {
        FormExtensions.Superposition().Should().Be(Form.Void);
    }

    [Fact]
    public void Superposition_AllMark_ReturnsMark()
    {
        FormExtensions.Superposition(
            (Form.Mark, 1.0),
            (Form.Mark, 1.0),
            (Form.Mark, 1.0)
        ).Should().Be(Form.Mark);
    }

    [Fact]
    public void Superposition_AllVoid_ReturnsVoid()
    {
        FormExtensions.Superposition(
            (Form.Void, 1.0),
            (Form.Void, 1.0)
        ).Should().Be(Form.Void);
    }

    [Fact]
    public void Superposition_AnyImaginary_ReturnsImaginary()
    {
        FormExtensions.Superposition(
            (Form.Mark, 1.0),
            (Form.Imaginary, 0.5)
        ).Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Superposition_MixedWithoutConsensus_ReturnsImaginary()
    {
        FormExtensions.Superposition(
            (Form.Mark, 1.0),
            (Form.Void, 1.0)
        ).Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Superposition_StrongMarkConsensus_ReturnsMark()
    {
        FormExtensions.Superposition(
            (Form.Mark, 3.0),
            (Form.Void, 1.0)
        ).Should().Be(Form.Mark);
    }

    // --- ToResult ---

    [Fact]
    public void ToResult_Mark_ReturnsSuccess()
    {
        var result = Form.Mark.ToResult(42, "error");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToResult_Void_ReturnsFailure()
    {
        var result = Form.Void.ToResult(42, "below threshold");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ToResult_Imaginary_ReturnsUncertaintyFailure()
    {
        var result = Form.Imaginary.ToResult(42, "error");
        result.IsFailure.Should().BeTrue();
    }

    // --- ToOption ---

    [Fact]
    public void ToOption_Mark_ReturnsSome()
    {
        var opt = Form.Mark.ToOption("value");
        opt.HasValue.Should().BeTrue();
    }

    [Fact]
    public void ToOption_Void_ReturnsNone()
    {
        var opt = Form.Void.ToOption("value");
        opt.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToOption_Imaginary_ReturnsNone()
    {
        var opt = Form.Imaginary.ToOption("value");
        opt.HasValue.Should().BeFalse();
    }

    // --- ToTriState / ToBoolean ---

    [Fact]
    public void ToTriState_ReturnsCorrectState()
    {
        Form.Mark.ToTriState().Should().Be(TriState.Mark);
        Form.Void.ToTriState().Should().Be(TriState.Void);
        Form.Imaginary.ToTriState().Should().Be(TriState.Imaginary);
    }

    [Fact]
    public void ToBoolean_Mark_ReturnsTrue()
    {
        Form.Mark.ToBoolean().Should().BeTrue();
    }

    [Fact]
    public void ToBoolean_VoidOrImaginary_ReturnsFalse()
    {
        Form.Void.ToBoolean().Should().BeFalse();
        Form.Imaginary.ToBoolean().Should().BeFalse();
    }

    // --- Implies ---

    [Fact]
    public void Implies_MarkImpliesMark_ReturnsMark()
    {
        // T -> T = T
        Form.Mark.Implies(Form.Mark).Should().Be(Form.Mark);
    }

    [Fact]
    public void Implies_MarkImpliesVoid_ReturnsVoid()
    {
        // T -> F = F
        Form.Mark.Implies(Form.Void).Should().Be(Form.Void);
    }

    [Fact]
    public void Implies_VoidImpliesAnything_ReturnsMark()
    {
        // F -> X = T (vacuous truth)
        Form.Void.Implies(Form.Void).Should().Be(Form.Mark);
        Form.Void.Implies(Form.Mark).Should().Be(Form.Mark);
    }

    // --- Map ---

    [Fact]
    public void Map_AppliesTransformation()
    {
        var result = Form.Mark.Map(f => f.Not());
        result.Should().Be(Form.Void);
    }

    // --- FromOption / FromResult ---

    [Fact]
    public void FromOption_Some_ReturnsMark()
    {
        FormExtensions.FromOption(Option<int>.Some(42)).Should().Be(Form.Mark);
    }

    [Fact]
    public void FromOption_None_ReturnsVoid()
    {
        FormExtensions.FromOption(Option<int>.None()).Should().Be(Form.Void);
    }

    [Fact]
    public void FromResult_Success_ReturnsMark()
    {
        FormExtensions.FromResult(Result<int, string>.Success(42)).Should().Be(Form.Mark);
    }

    [Fact]
    public void FromResult_Failure_ReturnsVoid()
    {
        FormExtensions.FromResult(Result<int, string>.Failure("err")).Should().Be(Form.Void);
    }
}
