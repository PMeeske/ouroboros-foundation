// <copyright file="DreamTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;
using Ouroboros.Providers.Random;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="Dream"/> class.
/// </summary>
[Trait("Category", "Unit")]
public class DreamTests
{
    // --- Observe ---

    [Fact]
    public void Observe_Choice0_ReturnsVoid()
    {
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.Next(3)).Returns(0);
        var dream = new Dream(mockRandom.Object);

        var result = dream.Observe();

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void Observe_Choice1_ReturnsMark()
    {
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.Next(3)).Returns(1);
        var dream = new Dream(mockRandom.Object);

        var result = dream.Observe();

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void Observe_Choice2_ReturnsImaginary()
    {
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.Next(3)).Returns(2);
        mockRandom.Setup(r => r.NextDouble()).Returns(0.5);
        var dream = new Dream(mockRandom.Object);

        var result = dream.Observe();

        result.Should().Be(Form.Imaginary);
    }

    // --- ObserveWithBias ---

    [Fact]
    public void ObserveWithBias_RollBelowBias_ReturnsMark()
    {
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.3);
        var dream = new Dream(mockRandom.Object);

        var result = dream.ObserveWithBias(0.5);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void ObserveWithBias_RollInMiddleRange_ReturnsVoid()
    {
        var mockRandom = new Mock<IRandomProvider>();
        // bias=0.5, middle range is [0.5, 0.5 + (1-0.5)/2) = [0.5, 0.75)
        mockRandom.Setup(r => r.NextDouble()).Returns(0.6);
        var dream = new Dream(mockRandom.Object);

        var result = dream.ObserveWithBias(0.5);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void ObserveWithBias_RollAboveMiddleRange_ReturnsImaginary()
    {
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.SetupSequence(r => r.NextDouble())
            .Returns(0.9)  // first call: the roll
            .Returns(0.5); // second call: the phase
        var dream = new Dream(mockRandom.Object);

        var result = dream.ObserveWithBias(0.5);

        result.Should().Be(Form.Imaginary);
    }

    // --- Manifest ---

    [Fact]
    public void Manifest_ReturnsImaginaryForm()
    {
        var result = Dream.Manifest(Math.PI);

        result.Should().Be(Form.Imaginary);
    }

    // --- ToString ---

    [Fact]
    public void ToString_ReturnsDreamSymbol()
    {
        var dream = new Dream();

        dream.ToString().Should().Be("◇ (dream)");
    }

    // --- Default constructor uses CryptoRandomProvider ---

    [Fact]
    public void DefaultConstructor_DoesNotThrow()
    {
        var act = () => new Dream();

        act.Should().NotThrow();
    }
}
