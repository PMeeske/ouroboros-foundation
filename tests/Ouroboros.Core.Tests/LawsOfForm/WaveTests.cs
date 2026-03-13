using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class WaveTests
{
    [Fact]
    public void Sample_AtZero_ReturnsPhaseBasedValue()
    {
        var sut = new Wave(1.0, 0.0);

        sut.Sample(0.0).Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Sample_AtQuarterPeriod_ReturnsOne()
    {
        var sut = new Wave(1.0, 0.0);

        sut.Sample(0.25).Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void IsMarkedAt_PositiveAmplitude_ReturnsTrue()
    {
        var sut = new Wave(1.0, 0.0);

        sut.IsMarkedAt(0.25).Should().BeTrue();
    }

    [Fact]
    public void IsMarkedAt_NegativeAmplitude_ReturnsFalse()
    {
        var sut = new Wave(1.0, 0.0);

        sut.IsMarkedAt(0.75).Should().BeFalse();
    }

    [Fact]
    public void ToFormAt_PositiveAmplitude_ReturnsMark()
    {
        var sut = new Wave(1.0, 0.0);

        sut.ToFormAt(0.25).Should().Be(Form.Mark);
    }

    [Fact]
    public void ToFormAt_NegativeAmplitude_ReturnsVoid()
    {
        var sut = new Wave(1.0, 0.0);

        sut.ToFormAt(0.75).Should().Be(Form.Void);
    }

    [Fact]
    public void ToImaginary_ReturnsImaginaryForm()
    {
        var sut = new Wave(1.0, Math.PI / 4);

        var result = sut.ToImaginary();

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void RecordEquality_SameParams_AreEqual()
    {
        var a = new Wave(1.0, 0.5);
        var b = new Wave(1.0, 0.5);

        a.Should().Be(b);
    }
}
