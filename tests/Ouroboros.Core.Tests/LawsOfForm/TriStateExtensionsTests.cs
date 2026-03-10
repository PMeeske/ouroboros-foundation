// <copyright file="TriStateExtensionsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for <see cref="TriStateExtensions"/> which provides extension methods
/// for the TriState enum supporting hierarchical configuration resolution.
/// </summary>
[Trait("Category", "Unit")]
public class TriStateExtensionsTests
{
    // ──────────── ResolveChain ────────────

    [Fact]
    public void ResolveChain_FirstStateMark_ReturnsTrue()
    {
        bool result = TriStateExtensions.ResolveChain(false, TriState.Mark, TriState.Void);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResolveChain_FirstStateVoid_ReturnsFalse()
    {
        bool result = TriStateExtensions.ResolveChain(true, TriState.Void, TriState.Mark);

        result.Should().BeFalse();
    }

    [Fact]
    public void ResolveChain_FirstImaginarySecondMark_ReturnsTrue()
    {
        bool result = TriStateExtensions.ResolveChain(false, TriState.Imaginary, TriState.Mark);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResolveChain_AllImaginary_ReturnsSystemDefault()
    {
        bool resultTrue = TriStateExtensions.ResolveChain(true, TriState.Imaginary, TriState.Imaginary);
        bool resultFalse = TriStateExtensions.ResolveChain(false, TriState.Imaginary, TriState.Imaginary);

        resultTrue.Should().BeTrue();
        resultFalse.Should().BeFalse();
    }

    [Fact]
    public void ResolveChain_EmptyStates_ReturnsSystemDefault()
    {
        bool result = TriStateExtensions.ResolveChain(true);

        result.Should().BeTrue();
    }

    [Fact]
    public void ResolveChain_MultipleImaginaryThenVoid_ReturnsFalse()
    {
        bool result = TriStateExtensions.ResolveChain(
            true,
            TriState.Imaginary,
            TriState.Imaginary,
            TriState.Void);

        result.Should().BeFalse();
    }

    // ──────────── FromBool ────────────

    [Fact]
    public void FromBool_True_ReturnsMark()
    {
        TriStateExtensions.FromBool(true).Should().Be(TriState.Mark);
    }

    [Fact]
    public void FromBool_False_ReturnsVoid()
    {
        TriStateExtensions.FromBool(false).Should().Be(TriState.Void);
    }

    // ──────────── ToBoolOrNull ────────────

    [Fact]
    public void ToBoolOrNull_Mark_ReturnsTrue()
    {
        TriState.Mark.ToBoolOrNull().Should().BeTrue();
    }

    [Fact]
    public void ToBoolOrNull_Void_ReturnsFalse()
    {
        TriState.Void.ToBoolOrNull().Should().BeFalse();
    }

    [Fact]
    public void ToBoolOrNull_Imaginary_ReturnsNull()
    {
        TriState.Imaginary.ToBoolOrNull().Should().BeNull();
    }

    // ──────────── IsDefinite ────────────

    [Fact]
    public void IsDefinite_Mark_ReturnsTrue()
    {
        TriState.Mark.IsDefinite().Should().BeTrue();
    }

    [Fact]
    public void IsDefinite_Void_ReturnsTrue()
    {
        TriState.Void.IsDefinite().Should().BeTrue();
    }

    [Fact]
    public void IsDefinite_Imaginary_ReturnsFalse()
    {
        TriState.Imaginary.IsDefinite().Should().BeFalse();
    }

    // ──────────── IsImaginary ────────────

    [Fact]
    public void IsImaginary_Mark_ReturnsFalse()
    {
        TriState.Mark.IsImaginary().Should().BeFalse();
    }

    [Fact]
    public void IsImaginary_Void_ReturnsFalse()
    {
        TriState.Void.IsImaginary().Should().BeFalse();
    }

    [Fact]
    public void IsImaginary_Imaginary_ReturnsTrue()
    {
        TriState.Imaginary.IsImaginary().Should().BeTrue();
    }

    // ──────────── ToForm ────────────

    [Fact]
    public void ToForm_Mark_ReturnsFormMark()
    {
        TriState.Mark.ToForm().Should().Be(Form.Mark);
    }

    [Fact]
    public void ToForm_Void_ReturnsFormVoid()
    {
        TriState.Void.ToForm().Should().Be(Form.Void);
    }

    [Fact]
    public void ToForm_Imaginary_ReturnsFormImaginary()
    {
        TriState.Imaginary.ToForm().Should().Be(Form.Imaginary);
    }

    // ──────────── FromNullable ────────────

    [Fact]
    public void FromNullable_True_ReturnsMark()
    {
        TriStateExtensions.FromNullable(true).Should().Be(TriState.Mark);
    }

    [Fact]
    public void FromNullable_False_ReturnsVoid()
    {
        TriStateExtensions.FromNullable(false).Should().Be(TriState.Void);
    }

    [Fact]
    public void FromNullable_Null_ReturnsImaginary()
    {
        TriStateExtensions.FromNullable(null).Should().Be(TriState.Imaginary);
    }

    // ──────────── ToNullable ────────────

    [Fact]
    public void ToNullable_Mark_ReturnsTrue()
    {
        TriState.Mark.ToNullable().Should().Be(true);
    }

    [Fact]
    public void ToNullable_Void_ReturnsFalse()
    {
        TriState.Void.ToNullable().Should().Be(false);
    }

    [Fact]
    public void ToNullable_Imaginary_ReturnsNull()
    {
        TriState.Imaginary.ToNullable().Should().BeNull();
    }

    // ──────────── Resolve ────────────

    [Fact]
    public void Resolve_Mark_ReturnsTrue()
    {
        TriState.Mark.Resolve(false).Should().BeTrue();
    }

    [Fact]
    public void Resolve_Void_ReturnsFalse()
    {
        TriState.Void.Resolve(true).Should().BeFalse();
    }

    [Fact]
    public void Resolve_Imaginary_ReturnsParentValue()
    {
        TriState.Imaginary.Resolve(true).Should().BeTrue();
        TriState.Imaginary.Resolve(false).Should().BeFalse();
    }

    // ──────────── And ────────────

    [Theory]
    [InlineData(TriState.Mark, TriState.Mark, TriState.Mark)]
    [InlineData(TriState.Mark, TriState.Void, TriState.Void)]
    [InlineData(TriState.Void, TriState.Mark, TriState.Void)]
    [InlineData(TriState.Void, TriState.Void, TriState.Void)]
    [InlineData(TriState.Mark, TriState.Imaginary, TriState.Imaginary)]
    [InlineData(TriState.Imaginary, TriState.Mark, TriState.Imaginary)]
    [InlineData(TriState.Imaginary, TriState.Void, TriState.Imaginary)]
    [InlineData(TriState.Void, TriState.Imaginary, TriState.Imaginary)]
    [InlineData(TriState.Imaginary, TriState.Imaginary, TriState.Imaginary)]
    public void And_ReturnsExpectedResult(TriState left, TriState right, TriState expected)
    {
        left.And(right).Should().Be(expected);
    }

    // ──────────── Or ────────────

    [Theory]
    [InlineData(TriState.Mark, TriState.Mark, TriState.Mark)]
    [InlineData(TriState.Mark, TriState.Void, TriState.Mark)]
    [InlineData(TriState.Void, TriState.Mark, TriState.Mark)]
    [InlineData(TriState.Void, TriState.Void, TriState.Void)]
    [InlineData(TriState.Mark, TriState.Imaginary, TriState.Imaginary)]
    [InlineData(TriState.Imaginary, TriState.Mark, TriState.Imaginary)]
    [InlineData(TriState.Imaginary, TriState.Void, TriState.Imaginary)]
    [InlineData(TriState.Void, TriState.Imaginary, TriState.Imaginary)]
    [InlineData(TriState.Imaginary, TriState.Imaginary, TriState.Imaginary)]
    public void Or_ReturnsExpectedResult(TriState left, TriState right, TriState expected)
    {
        left.Or(right).Should().Be(expected);
    }

    // ──────────── Alias consistency ────────────

    [Fact]
    public void Aliases_OnEqualsMarkOffEqualsVoidInheritEqualsImaginary()
    {
        TriState.On.Should().Be(TriState.Mark);
        TriState.Off.Should().Be(TriState.Void);
        TriState.Inherit.Should().Be(TriState.Imaginary);
    }

    // ──────────── Roundtrip conversions ────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FromBool_ToBoolOrNull_Roundtrips(bool value)
    {
        TriState state = TriStateExtensions.FromBool(value);
        bool? result = state.ToBoolOrNull();

        result.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public void FromNullable_ToNullable_Roundtrips(bool? value)
    {
        TriState state = TriStateExtensions.FromNullable(value);
        bool? result = state.ToNullable();

        result.Should().Be(value);
    }
}
