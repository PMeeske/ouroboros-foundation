using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class ImaginationTests
{
    [Fact]
    public void I_ReturnsImaginaryForm()
    {
        Imagination.I.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void SelfReference_ReturnsReEntryForm()
    {
        var result = Imagination.SelfReference("test");

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void SelfReference_WithoutName_ReturnsReEntryForm()
    {
        var result = Imagination.SelfReference();

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Oscillate_CreateOscillator_ReturnsCorrectStates()
    {
        var osc = Imagination.Oscillate(Form.Mark, Form.Void);

        osc.AtTime(0).Should().Be(Form.Mark);
        osc.AtTime(1).Should().Be(Form.Void);
    }

    [Fact]
    public void CreateWave_DefaultParams_ReturnsWave()
    {
        var wave = Imagination.CreateWave();

        wave.Frequency.Should().Be(1.0);
        wave.Phase.Should().Be(0.0);
    }

    [Fact]
    public void CreateWave_CustomParams_ReturnsWaveWithParams()
    {
        var wave = Imagination.CreateWave(2.0, Math.PI);

        wave.Frequency.Should().Be(2.0);
        wave.Phase.Should().Be(Math.PI);
    }

    [Fact]
    public void Apply_Void_ReturnsImaginary()
    {
        var result = Imagination.Apply(Form.Void);

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Apply_Mark_ReturnsImaginaryWithPhaseShift()
    {
        var result = Imagination.Apply(Form.Mark);

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Conjugate_RealForm_ReturnsSelf()
    {
        Imagination.Conjugate(Form.Mark).Should().Be(Form.Mark);
        Imagination.Conjugate(Form.Void).Should().Be(Form.Void);
    }

    [Fact]
    public void Magnitude_Void_ReturnsZero()
    {
        Imagination.Magnitude(Form.Void).Should().Be(0.0);
    }

    [Fact]
    public void Magnitude_Mark_ReturnsOne()
    {
        Imagination.Magnitude(Form.Mark).Should().Be(1.0);
    }

    [Fact]
    public void Magnitude_Imaginary_ReturnsOne()
    {
        Imagination.Magnitude(Form.Imaginary).Should().Be(1.0);
    }

    [Fact]
    public void Phase_Void_ReturnsZero()
    {
        Imagination.Phase(Form.Void).Should().Be(0.0);
    }

    [Fact]
    public void Phase_Mark_ReturnsPi()
    {
        Imagination.Phase(Form.Mark).Should().BeApproximately(Math.PI, 1e-10);
    }

    [Fact]
    public void Project_RealForm_ReturnsSelf()
    {
        Imagination.Project(Form.Mark).Should().Be(Form.Mark);
        Imagination.Project(Form.Void).Should().Be(Form.Void);
    }

    [Fact]
    public void Sample_RealForm_IsConstant()
    {
        Imagination.Sample(Form.Mark, 0).Should().Be(Form.Mark);
        Imagination.Sample(Form.Mark, 1).Should().Be(Form.Mark);
    }

    [Fact]
    public void Sample_ImaginaryForm_Oscillates()
    {
        Imagination.Sample(Form.Imaginary, 0).Should().Be(Form.Void);
        Imagination.Sample(Form.Imaginary, 1).Should().Be(Form.Mark);
    }

    [Fact]
    public void CreateDream_ReturnsDreamInstance()
    {
        var dream = Imagination.CreateDream();

        dream.Should().NotBeNull();
    }

    [Fact]
    public void Superimpose_TwoRealForms_UsesIndication()
    {
        var result = Imagination.Superimpose(Form.Mark, Form.Void);

        // Form.Call with Mark and Void should produce a result
        result.Should().NotBe(default(Form));
    }
}
