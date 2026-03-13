using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class OscillatorTests
{
    [Fact]
    public void AtTime_EvenTime_ReturnsStateA()
    {
        var sut = new Oscillator(Form.Mark, Form.Void);

        sut.AtTime(0).Should().Be(Form.Mark);
        sut.AtTime(2).Should().Be(Form.Mark);
        sut.AtTime(4).Should().Be(Form.Mark);
    }

    [Fact]
    public void AtTime_OddTime_ReturnsStateB()
    {
        var sut = new Oscillator(Form.Mark, Form.Void);

        sut.AtTime(1).Should().Be(Form.Void);
        sut.AtTime(3).Should().Be(Form.Void);
        sut.AtTime(5).Should().Be(Form.Void);
    }

    [Fact]
    public void Period_IsAlwaysTwo()
    {
        Oscillator.Period.Should().Be(2);
    }

    [Fact]
    public void ToImaginary_ReturnsImaginaryForm()
    {
        Oscillator.ToImaginary().Should().Be(Form.Imaginary);
    }

    [Fact]
    public void RecordEquality_SameStates_AreEqual()
    {
        var a = new Oscillator(Form.Mark, Form.Void);
        var b = new Oscillator(Form.Mark, Form.Void);

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentStates_AreNotEqual()
    {
        var a = new Oscillator(Form.Mark, Form.Void);
        var b = new Oscillator(Form.Void, Form.Mark);

        a.Should().NotBe(b);
    }
}
