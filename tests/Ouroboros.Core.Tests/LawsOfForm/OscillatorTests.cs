using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class OscillatorTests
{
    [Fact]
    public void AtTime_EvenTime_ReturnsStateA()
    {
        var sut = new Oscillator(LoF.Mark, LoF.Void);

        sut.AtTime(0).Should().Be(LoF.Mark);
        sut.AtTime(2).Should().Be(LoF.Mark);
        sut.AtTime(4).Should().Be(LoF.Mark);
    }

    [Fact]
    public void AtTime_OddTime_ReturnsStateB()
    {
        var sut = new Oscillator(LoF.Mark, LoF.Void);

        sut.AtTime(1).Should().Be(LoF.Void);
        sut.AtTime(3).Should().Be(LoF.Void);
        sut.AtTime(5).Should().Be(LoF.Void);
    }

    [Fact]
    public void Period_IsAlwaysTwo()
    {
        Oscillator.Period.Should().Be(2);
    }

    [Fact]
    public void ToImaginary_ReturnsImaginaryForm()
    {
        Oscillator.ToImaginary().Should().Be(LoF.Imaginary);
    }

    [Fact]
    public void RecordEquality_SameStates_AreEqual()
    {
        var a = new Oscillator(LoF.Mark, LoF.Void);
        var b = new Oscillator(LoF.Mark, LoF.Void);

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentStates_AreNotEqual()
    {
        var a = new Oscillator(LoF.Mark, LoF.Void);
        var b = new Oscillator(LoF.Void, LoF.Mark);

        a.Should().NotBe(b);
    }
}
