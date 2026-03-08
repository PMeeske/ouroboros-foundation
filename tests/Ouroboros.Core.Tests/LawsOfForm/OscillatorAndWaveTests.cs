// <copyright file="OscillatorAndWaveTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for Oscillator and Wave types representing temporal dynamics in Laws of Form.
/// </summary>
[Trait("Category", "Unit")]
public class OscillatorAndWaveTests
{
    // --- Oscillator ---

    [Fact]
    public void Oscillator_AtEvenTime_ReturnsStateA()
    {
        // Arrange
        var oscillator = new Oscillator(Form.Mark, Form.Void);

        // Act & Assert
        oscillator.AtTime(0).Should().Be(Form.Mark);
        oscillator.AtTime(2).Should().Be(Form.Mark);
        oscillator.AtTime(4).Should().Be(Form.Mark);
    }

    [Fact]
    public void Oscillator_AtOddTime_ReturnsStateB()
    {
        // Arrange
        var oscillator = new Oscillator(Form.Mark, Form.Void);

        // Act & Assert
        oscillator.AtTime(1).Should().Be(Form.Void);
        oscillator.AtTime(3).Should().Be(Form.Void);
        oscillator.AtTime(5).Should().Be(Form.Void);
    }

    [Fact]
    public void Oscillator_Period_IsAlwaysTwo()
    {
        var oscillator = new Oscillator(Form.Mark, Form.Imaginary);
        Oscillator.Period.Should().Be(2);
    }

    [Fact]
    public void Oscillator_ToImaginary_ReturnsImaginaryForm()
    {
        var oscillator = new Oscillator(Form.Mark, Form.Void);
        Oscillator.ToImaginary().Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Oscillator_NegativeTime_StillAlternates()
    {
        var oscillator = new Oscillator(Form.Mark, Form.Void);

        // Negative even -> StateA, negative odd -> StateB
        oscillator.AtTime(-2).Should().Be(Form.Mark);
        oscillator.AtTime(-1).Should().Be(Form.Void);
    }

    [Fact]
    public void Oscillator_WithImaginaryStates_Oscillates()
    {
        var oscillator = new Oscillator(Form.Imaginary, Form.Mark);

        oscillator.AtTime(0).Should().Be(Form.Imaginary);
        oscillator.AtTime(1).Should().Be(Form.Mark);
    }

    // --- Wave ---

    [Fact]
    public void Wave_Sample_ReturnsSineWaveValue()
    {
        // Arrange: frequency=1, phase=0 -> sin(2*pi*t)
        var wave = new Wave(1.0, 0.0);

        // At t=0, sin(0) = 0
        wave.Sample(0.0).Should().BeApproximately(0.0, 1e-10);

        // At t=0.25, sin(pi/2) = 1
        wave.Sample(0.25).Should().BeApproximately(1.0, 1e-10);

        // At t=0.5, sin(pi) ~= 0
        wave.Sample(0.5).Should().BeApproximately(0.0, 1e-10);
    }

    [Fact]
    public void Wave_IsMarkedAt_PositiveAmplitude_ReturnsTrue()
    {
        var wave = new Wave(1.0, 0.0);

        // At t=0.25, amplitude is ~1 (positive)
        wave.IsMarkedAt(0.25).Should().BeTrue();
    }

    [Fact]
    public void Wave_IsMarkedAt_NegativeAmplitude_ReturnsFalse()
    {
        var wave = new Wave(1.0, 0.0);

        // At t=0.75, amplitude is ~-1 (negative)
        wave.IsMarkedAt(0.75).Should().BeFalse();
    }

    [Fact]
    public void Wave_ToFormAt_PositiveAmplitude_ReturnsMark()
    {
        var wave = new Wave(1.0, 0.0);
        wave.ToFormAt(0.25).Should().Be(Form.Mark);
    }

    [Fact]
    public void Wave_ToFormAt_NegativeAmplitude_ReturnsVoid()
    {
        var wave = new Wave(1.0, 0.0);
        wave.ToFormAt(0.75).Should().Be(Form.Void);
    }

    [Fact]
    public void Wave_ToImaginary_ReturnsImaginaryForm()
    {
        var wave = new Wave(1.0, Math.PI / 4);
        wave.ToImaginary().Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Wave_WithPhaseOffset_ShiftsSample()
    {
        // Phase=pi/2 shifts the wave by quarter period
        var wave = new Wave(1.0, Math.PI / 2);

        // At t=0, sin(pi/2) = 1
        wave.Sample(0.0).Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Wave_SampleValueRange_BetweenMinusOneAndOne()
    {
        var wave = new Wave(2.0, 0.5);

        for (double t = 0; t < 1.0; t += 0.01)
        {
            var sample = wave.Sample(t);
            sample.Should().BeGreaterThanOrEqualTo(-1.0);
            sample.Should().BeLessThanOrEqualTo(1.0);
        }
    }

    [Fact]
    public void Wave_HigherFrequency_OscillatesFaster()
    {
        var lowFreq = new Wave(1.0, 0.0);
        var highFreq = new Wave(4.0, 0.0);

        // At t=0.0625, low freq is still near 0, high freq completes quarter period
        var lowSample = Math.Abs(lowFreq.Sample(0.0625));
        var highSample = Math.Abs(highFreq.Sample(0.0625));

        highSample.Should().BeGreaterThan(lowSample);
    }
}
