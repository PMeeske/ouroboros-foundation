using Moq;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Providers.Random;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class DreamTests
{
    [Fact]
    public void Observe_WhenRandomReturnsZero_ReturnsVoid()
    {
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.Next(3)).Returns(0);
        var sut = new Dream(mockRandom.Object);

        var result = sut.Observe();

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void Observe_WhenRandomReturnsOne_ReturnsMark()
    {
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.Next(3)).Returns(1);
        var sut = new Dream(mockRandom.Object);

        var result = sut.Observe();

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void Observe_WhenRandomReturnsTwo_ReturnsImaginary()
    {
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.Next(3)).Returns(2);
        mockRandom.Setup(r => r.NextDouble()).Returns(0.5);
        var sut = new Dream(mockRandom.Object);

        var result = sut.Observe();

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void ObserveWithBias_HighBias_ReturnsMark()
    {
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.1);
        var sut = new Dream(mockRandom.Object);

        var result = sut.ObserveWithBias(0.9);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void ObserveWithBias_MidRoll_ReturnsVoid()
    {
        var mockRandom = new Mock<IRandomProvider>();
        // bias = 0.5, roll = 0.6 => 0.6 < 0.5 + (0.5/2) = 0.75 => Void
        mockRandom.Setup(r => r.NextDouble()).Returns(0.6);
        var sut = new Dream(mockRandom.Object);

        var result = sut.ObserveWithBias(0.5);

        result.Should().Be(Form.Void);
    }

    [Fact]
    public void ObserveWithBias_HighRoll_ReturnsImaginary()
    {
        var mockRandom = new Mock<IRandomProvider>();
        var callCount = 0;
        // bias = 0.5, roll = 0.8 => 0.8 >= 0.75 => Imaginary branch, then NextDouble for phase
        mockRandom.Setup(r => r.NextDouble()).Returns(() => callCount++ == 0 ? 0.8 : 0.25);
        var sut = new Dream(mockRandom.Object);

        var result = sut.ObserveWithBias(0.5);

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Manifest_ReturnsImaginaryFormAtPhase()
    {
        var result = Dream.Manifest(Math.PI);

        result.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void ToString_ReturnsDreamSymbol()
    {
        var sut = new Dream();

        sut.ToString().Should().Contain("dream");
    }
}
