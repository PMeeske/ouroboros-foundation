using Ouroboros.Domain.Persistence;

namespace Ouroboros.Tests.Persistence;

[Trait("Category", "Unit")]
public class ThoughtStatisticsTests
{
    [Fact]
    public void Construction_DefaultValues_AreCorrect()
    {
        var stats = new ThoughtStatistics();

        stats.TotalCount.Should().Be(0);
        stats.CountByType.Should().BeEmpty();
        stats.CountByOrigin.Should().BeEmpty();
        stats.AverageConfidence.Should().Be(0);
        stats.AverageRelevance.Should().Be(0);
        stats.EarliestThought.Should().BeNull();
        stats.LatestThought.Should().BeNull();
        stats.ChainCount.Should().Be(0);
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var earliest = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var latest = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var stats = new ThoughtStatistics
        {
            TotalCount = 100,
            CountByType = new Dictionary<string, int>
            {
                ["Observation"] = 50,
                ["Analytical"] = 30,
                ["Curiosity"] = 20
            },
            CountByOrigin = new Dictionary<string, int>
            {
                ["Reactive"] = 60,
                ["Autonomous"] = 40
            },
            AverageConfidence = 0.75,
            AverageRelevance = 0.82,
            EarliestThought = earliest,
            LatestThought = latest,
            ChainCount = 15
        };

        stats.TotalCount.Should().Be(100);
        stats.CountByType.Should().HaveCount(3);
        stats.CountByType["Observation"].Should().Be(50);
        stats.CountByOrigin.Should().HaveCount(2);
        stats.AverageConfidence.Should().Be(0.75);
        stats.AverageRelevance.Should().Be(0.82);
        stats.EarliestThought.Should().Be(earliest);
        stats.LatestThought.Should().Be(latest);
        stats.ChainCount.Should().Be(15);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var s1 = new ThoughtStatistics { TotalCount = 5, AverageConfidence = 0.8 };
        var s2 = new ThoughtStatistics { TotalCount = 5, AverageConfidence = 0.8 };

        s1.Should().Be(s2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var stats = new ThoughtStatistics { TotalCount = 10 };
        var modified = stats with { TotalCount = 20, ChainCount = 5 };

        modified.TotalCount.Should().Be(20);
        modified.ChainCount.Should().Be(5);
        stats.TotalCount.Should().Be(10);
    }
}
