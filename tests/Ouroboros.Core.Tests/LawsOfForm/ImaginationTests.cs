// <copyright file="ImaginationTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for <see cref="Imagination"/> which provides operations for imaginary forms
/// in the Laws of Form calculus.
/// </summary>
[Trait("Category", "Unit")]
public class ImaginationTests
{
    // ──────────── I (Imaginary constant) ────────────

    [Fact]
    public void I_ReturnsImaginaryForm()
    {
        Imagination.I.Should().Be(Form.Imaginary);
        Imagination.I.IsImaginary().Should().BeTrue();
    }

    // ──────────── SelfReference ────────────

    [Fact]
    public void SelfReference_ReturnsImaginaryForm()
    {
        Form result = Imagination.SelfReference();

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void SelfReference_WithName_ReturnsImaginaryForm()
    {
        Form result = Imagination.SelfReference("f");

        result.IsImaginary().Should().BeTrue();
    }

    // ──────────── Oscillate ────────────

    [Fact]
    public void Oscillate_CreatesOscillatorWithCorrectStates()
    {
        Oscillator osc = Imagination.Oscillate(Form.Mark, Form.Void);

        osc.StateA.Should().Be(Form.Mark);
        osc.StateB.Should().Be(Form.Void);
    }

    [Fact]
    public void Oscillate_AtEvenTime_ReturnsStateA()
    {
        Oscillator osc = Imagination.Oscillate(Form.Mark, Form.Void);

        osc.AtTime(0).Should().Be(Form.Mark);
        osc.AtTime(2).Should().Be(Form.Mark);
        osc.AtTime(4).Should().Be(Form.Mark);
    }

    [Fact]
    public void Oscillate_AtOddTime_ReturnsStateB()
    {
        Oscillator osc = Imagination.Oscillate(Form.Mark, Form.Void);

        osc.AtTime(1).Should().Be(Form.Void);
        osc.AtTime(3).Should().Be(Form.Void);
        osc.AtTime(5).Should().Be(Form.Void);
    }

    // ──────────── CreateWave ────────────

    [Fact]
    public void CreateWave_DefaultParameters_CreatesWaveWithFrequency1Phase0()
    {
        Wave wave = Imagination.CreateWave();

        wave.Frequency.Should().Be(1.0);
        wave.Phase.Should().Be(0.0);
    }

    [Fact]
    public void CreateWave_WithParameters_CreatesCorrectWave()
    {
        Wave wave = Imagination.CreateWave(frequency: 2.5, phase: Math.PI / 4);

        wave.Frequency.Should().Be(2.5);
        wave.Phase.Should().Be(Math.PI / 4);
    }

    [Fact]
    public void CreateWave_SampleReturnsValueBetweenNegOneAndOne()
    {
        Wave wave = Imagination.CreateWave();

        for (double t = 0; t < 2.0; t += 0.1)
        {
            double sample = wave.Sample(t);
            sample.Should().BeInRange(-1.0, 1.0);
        }
    }

    // ──────────── Superimpose ────────────

    [Fact]
    public void Superimpose_BothRealForms_UsesCallLogic()
    {
        Form result = Imagination.Superimpose(Form.Mark, Form.Void);

        // Both real -> form1.Call(form2) => Mark.Call(Void) => Mark (Or logic)
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void Superimpose_BothVoid_ReturnsVoid()
    {
        Form result = Imagination.Superimpose(Form.Void, Form.Void);

        // Void.Call(Void) => Void.Or(Void) = Void
        result.Should().Be(Form.Void);
    }

    [Fact]
    public void Superimpose_OneImaginary_ReturnsImaginary()
    {
        Form result1 = Imagination.Superimpose(Form.Imaginary, Form.Mark);
        Form result2 = Imagination.Superimpose(Form.Void, Form.Imaginary);

        result1.IsImaginary().Should().BeTrue();
        result2.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Superimpose_BothImaginary_ReturnsImaginary()
    {
        Form result = Imagination.Superimpose(Form.Imaginary, Form.Imaginary);

        result.IsImaginary().Should().BeTrue();
    }

    // ──────────── Apply ────────────

    [Fact]
    public void Apply_Void_ReturnsImaginary()
    {
        Form result = Imagination.Apply(Form.Void);

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Apply_Mark_ReturnsImaginary()
    {
        Form result = Imagination.Apply(Form.Mark);

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Apply_Imaginary_ReturnsImaginary()
    {
        Form result = Imagination.Apply(Form.Imaginary);

        result.IsImaginary().Should().BeTrue();
    }

    // ──────────── Conjugate ────────────

    [Fact]
    public void Conjugate_ImaginaryForm_ReturnsImaginary()
    {
        Form result = Imagination.Conjugate(Form.Imaginary);

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Conjugate_Mark_ReturnsMark()
    {
        Form result = Imagination.Conjugate(Form.Mark);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void Conjugate_Void_ReturnsVoid()
    {
        Form result = Imagination.Conjugate(Form.Void);

        result.Should().Be(Form.Void);
    }

    // ──────────── Magnitude ────────────

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

    // ──────────── Phase ────────────

    [Fact]
    public void Phase_Void_ReturnsZero()
    {
        Imagination.Phase(Form.Void).Should().Be(0.0);
    }

    [Fact]
    public void Phase_Mark_ReturnsPi()
    {
        Imagination.Phase(Form.Mark).Should().Be(Math.PI);
    }

    [Fact]
    public void Phase_Imaginary_ReturnsPhaseValue()
    {
        // Form.Imaginary evaluates to ImaginaryForm with phase 0.0
        double phase = Imagination.Phase(Form.Imaginary);
        phase.Should().Be(0.0);
    }

    // ──────────── Project ────────────

    [Fact]
    public void Project_ImaginaryForm_ReturnsVoidOrMark()
    {
        // Imaginary with phase 0 -> normalized phase 0 < PI -> Void
        Form result = Imagination.Project(Form.Imaginary);

        result.Should().BeOneOf(Form.Void, Form.Mark);
    }

    [Fact]
    public void Project_Mark_ReturnsMark()
    {
        Form result = Imagination.Project(Form.Mark);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void Project_Void_ReturnsVoid()
    {
        Form result = Imagination.Project(Form.Void);

        result.Should().Be(Form.Void);
    }

    // ──────────── Sample ────────────

    [Fact]
    public void Sample_ImaginaryAtEvenTime_ReturnsVoid()
    {
        Form result = Imagination.Sample(Form.Imaginary, 0);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void Sample_ImaginaryAtOddTime_ReturnsMark()
    {
        Form result = Imagination.Sample(Form.Imaginary, 1);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void Sample_RealFormAtAnyTime_ReturnsSameForm()
    {
        Imagination.Sample(Form.Mark, 0).Should().Be(Form.Mark);
        Imagination.Sample(Form.Mark, 1).Should().Be(Form.Mark);
        Imagination.Sample(Form.Void, 0).Should().Be(Form.Void);
        Imagination.Sample(Form.Void, 1).Should().Be(Form.Void);
    }

    [Fact]
    public void Sample_ImaginaryAlternates()
    {
        var results = Enumerable.Range(0, 6)
            .Select(t => Imagination.Sample(Form.Imaginary, t))
            .ToList();

        // Even times: Void, Odd times: Mark
        results[0].Should().Be(Form.Void);
        results[1].Should().Be(Form.Mark);
        results[2].Should().Be(Form.Void);
        results[3].Should().Be(Form.Mark);
        results[4].Should().Be(Form.Void);
        results[5].Should().Be(Form.Mark);
    }

    // ──────────── CreateDream ────────────

    [Fact]
    public void CreateDream_ReturnsNonNullDream()
    {
        Dream dream = Imagination.CreateDream();

        dream.Should().NotBeNull();
    }

    [Fact]
    public void CreateDream_Observe_ReturnsValidForm()
    {
        Dream dream = Imagination.CreateDream();

        // Observe multiple times - each should be valid
        for (int i = 0; i < 10; i++)
        {
            Form form = dream.Observe();
            (form.IsMark() || form.IsVoid() || form.IsImaginary()).Should().BeTrue();
        }
    }

    [Fact]
    public void Dream_ToString_ReturnsExpectedString()
    {
        Dream dream = Imagination.CreateDream();

        dream.ToString().Should().Contain("dream");
    }

    [Fact]
    public void Dream_Manifest_ReturnsImaginaryForm()
    {
        Form form = Dream.Manifest(Math.PI / 2);

        form.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Dream_ObserveWithBias_HighBias_FavorsMark()
    {
        Dream dream = Imagination.CreateDream();
        int markCount = 0;
        int trials = 100;

        for (int i = 0; i < trials; i++)
        {
            Form form = dream.ObserveWithBias(0.9);
            if (form.IsMark()) markCount++;
        }

        // With bias 0.9, should get many marks
        markCount.Should().BeGreaterThan(50);
    }
}
