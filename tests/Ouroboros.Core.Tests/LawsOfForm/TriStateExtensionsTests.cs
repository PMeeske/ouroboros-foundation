using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class TriStateExtensionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FromBool_ConvertsCorrectly(bool value)
    {
        var result = TriStateExtensions.FromBool(value);

        result.Should().Be(value ? TriState.Mark : TriState.Void);
    }

    [Theory]
    [InlineData(TriState.Mark, true)]
    [InlineData(TriState.Void, false)]
    [InlineData(TriState.Imaginary, null)]
    public void ToBoolOrNull_ConvertsCorrectly(TriState state, bool? expected)
    {
        state.ToBoolOrNull().Should().Be(expected);
    }

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

    [Theory]
    [InlineData(true, TriState.Mark)]
    [InlineData(false, TriState.Void)]
    public void FromNullable_WithValue_ConvertsCorrectly(bool value, TriState expected)
    {
        TriStateExtensions.FromNullable(value).Should().Be(expected);
    }

    [Fact]
    public void FromNullable_Null_ReturnsImaginary()
    {
        TriStateExtensions.FromNullable(null).Should().Be(TriState.Imaginary);
    }

    [Theory]
    [InlineData(TriState.Mark, true)]
    [InlineData(TriState.Void, false)]
    [InlineData(TriState.Imaginary, null)]
    public void ToNullable_ConvertsCorrectly(TriState state, bool? expected)
    {
        state.ToNullable().Should().Be(expected);
    }

    [Theory]
    [InlineData(TriState.Mark, true, true)]
    [InlineData(TriState.Mark, false, true)]
    [InlineData(TriState.Void, true, false)]
    [InlineData(TriState.Void, false, false)]
    [InlineData(TriState.Imaginary, true, true)]
    [InlineData(TriState.Imaginary, false, false)]
    public void Resolve_UsesParentForImaginary(TriState state, bool parent, bool expected)
    {
        state.Resolve(parent).Should().Be(expected);
    }

    [Fact]
    public void ResolveChain_FirstDefiniteWins()
    {
        TriStateExtensions.ResolveChain(false, TriState.Imaginary, TriState.Mark, TriState.Void)
            .Should().BeTrue();
    }

    [Fact]
    public void ResolveChain_AllImaginary_FallsBackToDefault()
    {
        TriStateExtensions.ResolveChain(true, TriState.Imaginary, TriState.Imaginary)
            .Should().BeTrue();

        TriStateExtensions.ResolveChain(false, TriState.Imaginary, TriState.Imaginary)
            .Should().BeFalse();
    }

    // --- And/Or logic ---

    [Theory]
    [InlineData(TriState.Mark, TriState.Mark, TriState.Mark)]
    [InlineData(TriState.Mark, TriState.Void, TriState.Void)]
    [InlineData(TriState.Void, TriState.Void, TriState.Void)]
    [InlineData(TriState.Mark, TriState.Imaginary, TriState.Imaginary)]
    [InlineData(TriState.Imaginary, TriState.Void, TriState.Imaginary)]
    public void And_ReturnsExpected(TriState left, TriState right, TriState expected)
    {
        left.And(right).Should().Be(expected);
    }

    [Theory]
    [InlineData(TriState.Mark, TriState.Mark, TriState.Mark)]
    [InlineData(TriState.Mark, TriState.Void, TriState.Mark)]
    [InlineData(TriState.Void, TriState.Void, TriState.Void)]
    [InlineData(TriState.Mark, TriState.Imaginary, TriState.Imaginary)]
    [InlineData(TriState.Imaginary, TriState.Void, TriState.Imaginary)]
    public void Or_ReturnsExpected(TriState left, TriState right, TriState expected)
    {
        left.Or(right).Should().Be(expected);
    }
}
