using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class TriStateExtensionsTests
{
    // --- ResolveChain ---

    [Fact]
    public void ResolveChain_FirstMark_ReturnsTrue()
    {
        // Act
        bool result = TriStateExtensions.ResolveChain(false, TriState.Mark, TriState.Void);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ResolveChain_FirstVoid_ReturnsFalse()
    {
        // Act
        bool result = TriStateExtensions.ResolveChain(true, TriState.Void, TriState.Mark);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ResolveChain_AllImaginary_ReturnsDefault()
    {
        // Act
        bool result = TriStateExtensions.ResolveChain(true, TriState.Imaginary, TriState.Imaginary);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ResolveChain_ImaginaryThenMark_ReturnsMark()
    {
        // Act
        bool result = TriStateExtensions.ResolveChain(false, TriState.Imaginary, TriState.Mark);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ResolveChain_EmptyStates_ReturnsDefault()
    {
        // Act
        bool result = TriStateExtensions.ResolveChain(true);

        // Assert
        result.Should().BeTrue();
    }

    // --- FromBool ---

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

    // --- ToBoolOrNull ---

    [Fact]
    public void ToBoolOrNull_Mark_ReturnsTrue()
    {
        TriState.Mark.ToBoolOrNull().Should().Be(true);
    }

    [Fact]
    public void ToBoolOrNull_Void_ReturnsFalse()
    {
        TriState.Void.ToBoolOrNull().Should().Be(false);
    }

    [Fact]
    public void ToBoolOrNull_Imaginary_ReturnsNull()
    {
        TriState.Imaginary.ToBoolOrNull().Should().BeNull();
    }

    // --- IsDefinite ---

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

    // --- IsImaginary ---

    [Fact]
    public void IsImaginary_Imaginary_ReturnsTrue()
    {
        TriState.Imaginary.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void IsImaginary_Mark_ReturnsFalse()
    {
        TriState.Mark.IsImaginary().Should().BeFalse();
    }

    // --- ToForm ---

    [Fact]
    public void ToForm_Mark_ReturnsMarkForm()
    {
        TriState.Mark.ToForm().Should().Be(Form.Mark);
    }

    [Fact]
    public void ToForm_Void_ReturnsVoidForm()
    {
        TriState.Void.ToForm().Should().Be(Form.Void);
    }

    [Fact]
    public void ToForm_Imaginary_ReturnsImaginaryForm()
    {
        TriState.Imaginary.ToForm().Should().Be(Form.Imaginary);
    }

    // --- FromNullable ---

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

    // --- ToNullable ---

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

    // --- Resolve ---

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

    // --- And ---

    [Fact]
    public void And_MarkAndMark_ReturnsMark()
    {
        TriState.Mark.And(TriState.Mark).Should().Be(TriState.Mark);
    }

    [Fact]
    public void And_MarkAndVoid_ReturnsVoid()
    {
        TriState.Mark.And(TriState.Void).Should().Be(TriState.Void);
    }

    [Fact]
    public void And_VoidAndMark_ReturnsVoid()
    {
        TriState.Void.And(TriState.Mark).Should().Be(TriState.Void);
    }

    [Fact]
    public void And_VoidAndVoid_ReturnsVoid()
    {
        TriState.Void.And(TriState.Void).Should().Be(TriState.Void);
    }

    [Fact]
    public void And_ImaginaryPropagates()
    {
        TriState.Mark.And(TriState.Imaginary).Should().Be(TriState.Imaginary);
        TriState.Imaginary.And(TriState.Mark).Should().Be(TriState.Imaginary);
        TriState.Imaginary.And(TriState.Imaginary).Should().Be(TriState.Imaginary);
    }

    // --- Or ---

    [Fact]
    public void Or_MarkOrAnything_ReturnsMark()
    {
        TriState.Mark.Or(TriState.Mark).Should().Be(TriState.Mark);
        TriState.Mark.Or(TriState.Void).Should().Be(TriState.Mark);
    }

    [Fact]
    public void Or_VoidOrVoid_ReturnsVoid()
    {
        TriState.Void.Or(TriState.Void).Should().Be(TriState.Void);
    }

    [Fact]
    public void Or_ImaginaryPropagates()
    {
        TriState.Void.Or(TriState.Imaginary).Should().Be(TriState.Imaginary);
        TriState.Imaginary.Or(TriState.Void).Should().Be(TriState.Imaginary);
    }

    // --- Aliases ---

    [Fact]
    public void Aliases_AreEquivalent()
    {
        TriState.On.Should().Be(TriState.Mark);
        TriState.Off.Should().Be(TriState.Void);
        TriState.Inherit.Should().Be(TriState.Imaginary);
    }
}
