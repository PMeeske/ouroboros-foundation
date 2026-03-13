using Ouroboros.Domain.Reflection;

namespace Ouroboros.Tests.Reflection;

[Trait("Category", "Unit")]
public class CapabilityMapTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var scores = new Dictionary<CognitiveDimension, double>
        {
            [CognitiveDimension.Reasoning] = 0.9,
            [CognitiveDimension.Planning] = 0.7,
        };
        var strengths = new List<string> { "Logic" };
        var weaknesses = new List<string> { "Creativity" };

        var map = new CapabilityMap(scores, strengths, weaknesses);

        map.Scores.Should().HaveCount(2);
        map.Strengths.Should().ContainSingle();
        map.Weaknesses.Should().ContainSingle();
    }

    [Fact]
    public void OverallScore_WithScores_ShouldReturnAverage()
    {
        var scores = new Dictionary<CognitiveDimension, double>
        {
            [CognitiveDimension.Reasoning] = 0.8,
            [CognitiveDimension.Planning] = 0.6,
        };

        var map = new CapabilityMap(scores, new List<string>(), new List<string>());

        map.OverallScore.Should().BeApproximately(0.7, 0.001);
    }

    [Fact]
    public void OverallScore_EmptyScores_ShouldReturnZero()
    {
        var map = new CapabilityMap(
            new Dictionary<CognitiveDimension, double>(), new List<string>(), new List<string>());

        map.OverallScore.Should().Be(0.0);
    }

    [Fact]
    public void StrongestDimension_ShouldReturnHighestScored()
    {
        var scores = new Dictionary<CognitiveDimension, double>
        {
            [CognitiveDimension.Reasoning] = 0.5,
            [CognitiveDimension.Planning] = 0.9,
            [CognitiveDimension.Learning] = 0.7,
        };

        var map = new CapabilityMap(scores, new List<string>(), new List<string>());

        map.StrongestDimension.Should().Be(CognitiveDimension.Planning);
    }

    [Fact]
    public void WeakestDimension_ShouldReturnLowestScored()
    {
        var scores = new Dictionary<CognitiveDimension, double>
        {
            [CognitiveDimension.Reasoning] = 0.5,
            [CognitiveDimension.Planning] = 0.9,
            [CognitiveDimension.Learning] = 0.3,
        };

        var map = new CapabilityMap(scores, new List<string>(), new List<string>());

        map.WeakestDimension.Should().Be(CognitiveDimension.Learning);
    }

    [Fact]
    public void StrongestDimension_EmptyScores_ShouldReturnNull()
    {
        var map = new CapabilityMap(
            new Dictionary<CognitiveDimension, double>(), new List<string>(), new List<string>());

        map.StrongestDimension.Should().BeNull();
    }

    [Fact]
    public void WeakestDimension_EmptyScores_ShouldReturnNull()
    {
        var map = new CapabilityMap(
            new Dictionary<CognitiveDimension, double>(), new List<string>(), new List<string>());

        map.WeakestDimension.Should().BeNull();
    }
}
