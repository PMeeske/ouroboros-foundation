namespace Ouroboros.Tests.Domain.Reflection;

using Ouroboros.Domain.Reflection;

[Trait("Category", "Unit")]
public class CapabilityMapTests
{
    [Fact]
    public void OverallScore_WithScores_ReturnsAverage()
    {
        // Arrange
        var scores = new Dictionary<CognitiveDimension, double>
        {
            [CognitiveDimension.Reasoning] = 0.8,
            [CognitiveDimension.Planning] = 0.6,
            [CognitiveDimension.Learning] = 0.7,
        };

        // Act
        var map = new CapabilityMap(scores, new List<string>(), new List<string>());

        // Assert
        map.OverallScore.Should().BeApproximately(0.7, 0.001);
    }

    [Fact]
    public void OverallScore_EmptyScores_ReturnsZero()
    {
        // Act
        var map = new CapabilityMap(
            new Dictionary<CognitiveDimension, double>(),
            new List<string>(),
            new List<string>());

        // Assert
        map.OverallScore.Should().Be(0.0);
    }

    [Fact]
    public void StrongestDimension_ReturnsHighestScored()
    {
        // Arrange
        var scores = new Dictionary<CognitiveDimension, double>
        {
            [CognitiveDimension.Reasoning] = 0.5,
            [CognitiveDimension.Creativity] = 0.9,
            [CognitiveDimension.Memory] = 0.3,
        };

        // Act
        var map = new CapabilityMap(scores, new List<string>(), new List<string>());

        // Assert
        map.StrongestDimension.Should().Be(CognitiveDimension.Creativity);
    }

    [Fact]
    public void StrongestDimension_EmptyScores_ReturnsNull()
    {
        // Act
        var map = new CapabilityMap(
            new Dictionary<CognitiveDimension, double>(),
            new List<string>(),
            new List<string>());

        // Assert
        map.StrongestDimension.Should().BeNull();
    }

    [Fact]
    public void WeakestDimension_ReturnsLowestScored()
    {
        // Arrange
        var scores = new Dictionary<CognitiveDimension, double>
        {
            [CognitiveDimension.Reasoning] = 0.5,
            [CognitiveDimension.Creativity] = 0.9,
            [CognitiveDimension.Memory] = 0.3,
        };

        // Act
        var map = new CapabilityMap(scores, new List<string>(), new List<string>());

        // Assert
        map.WeakestDimension.Should().Be(CognitiveDimension.Memory);
    }

    [Fact]
    public void WeakestDimension_EmptyScores_ReturnsNull()
    {
        // Act
        var map = new CapabilityMap(
            new Dictionary<CognitiveDimension, double>(),
            new List<string>(),
            new List<string>());

        // Assert
        map.WeakestDimension.Should().BeNull();
    }

    [Fact]
    public void Strengths_AreStored()
    {
        // Arrange
        var strengths = new List<string> { "Reasoning", "Planning" };

        // Act
        var map = new CapabilityMap(
            new Dictionary<CognitiveDimension, double>(),
            strengths,
            new List<string>());

        // Assert
        map.Strengths.Should().HaveCount(2);
    }

    [Fact]
    public void Weaknesses_AreStored()
    {
        // Arrange
        var weaknesses = new List<string> { "Memory" };

        // Act
        var map = new CapabilityMap(
            new Dictionary<CognitiveDimension, double>(),
            new List<string>(),
            weaknesses);

        // Assert
        map.Weaknesses.Should().ContainSingle().Which.Should().Be("Memory");
    }
}
