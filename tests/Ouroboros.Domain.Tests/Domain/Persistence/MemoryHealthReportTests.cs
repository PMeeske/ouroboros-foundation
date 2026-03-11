using Ouroboros.Domain.Persistence;
using Ouroboros.Domain.Vectors;

namespace Ouroboros.Tests.Domain.Persistence;

[Trait("Category", "Unit")]
public class MemoryHealthReportTests
{
    private static MemoryStatistics CreateStats(
        int totalCollections = 5,
        long totalVectors = 1000,
        int healthy = 4,
        int unhealthy = 1,
        int links = 3) =>
        new(totalCollections, totalVectors, healthy, unhealthy, links,
            new Dictionary<ulong, int> { [384UL] = 3, [768UL] = 2 });

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var healed = new List<string> { "conversations" };
        var issues = new List<string> { "old_memories: corrupt index" };
        var stats = CreateStats();

        var report = new MemoryHealthReport(4, 1, healed, issues, stats);

        report.HealthyCollections.Should().Be(4);
        report.UnhealthyCollections.Should().Be(1);
        report.HealedCollections.Should().HaveCount(1);
        report.HealedCollections[0].Should().Be("conversations");
        report.RemainingIssues.Should().HaveCount(1);
        report.Statistics.Should().Be(stats);
    }

    [Fact]
    public void Constructor_AllHealthy_NoIssues()
    {
        var stats = CreateStats(healthy: 5, unhealthy: 0);

        var report = new MemoryHealthReport(5, 0, Array.Empty<string>(), Array.Empty<string>(), stats);

        report.HealthyCollections.Should().Be(5);
        report.UnhealthyCollections.Should().Be(0);
        report.HealedCollections.Should().BeEmpty();
        report.RemainingIssues.Should().BeEmpty();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var healed = new List<string> { "col1" };
        var issues = new List<string>();
        var stats = CreateStats();

        var a = new MemoryHealthReport(4, 1, healed, issues, stats);
        var b = new MemoryHealthReport(4, 1, healed, issues, stats);

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentHealthyCounts_NotEqual()
    {
        var stats = CreateStats();
        var empty = Array.Empty<string>();

        var a = new MemoryHealthReport(4, 1, empty, empty, stats);
        var b = new MemoryHealthReport(5, 0, empty, empty, stats);

        a.Should().NotBe(b);
    }

    [Fact]
    public void WithExpression_ChangesHealthyCollections()
    {
        var stats = CreateStats();
        var report = new MemoryHealthReport(3, 2, Array.Empty<string>(), Array.Empty<string>(), stats);
        var modified = report with { HealthyCollections = 5, UnhealthyCollections = 0 };

        modified.HealthyCollections.Should().Be(5);
        modified.UnhealthyCollections.Should().Be(0);
        report.HealthyCollections.Should().Be(3);
    }
}

[Trait("Category", "Unit")]
public class NeuroSymbolicStatsTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var oldest = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var newest = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var stats = new NeuroSymbolicStats(
            TotalThoughts: 100,
            TotalRelations: 50,
            TotalResults: 30,
            ThoughtsByType: new Dictionary<string, int> { ["Observation"] = 60, ["Analytical"] = 40 },
            RelationsByType: new Dictionary<string, int> { ["supports"] = 30, ["contradicts"] = 20 },
            ResultsByType: new Dictionary<string, int> { ["action"] = 20, ["insight"] = 10 },
            CausalChainCount: 15,
            AverageChainLength: 3.5,
            OldestThought: oldest,
            NewestThought: newest);

        stats.TotalThoughts.Should().Be(100);
        stats.TotalRelations.Should().Be(50);
        stats.TotalResults.Should().Be(30);
        stats.ThoughtsByType.Should().HaveCount(2);
        stats.RelationsByType.Should().HaveCount(2);
        stats.ResultsByType.Should().HaveCount(2);
        stats.CausalChainCount.Should().Be(15);
        stats.AverageChainLength.Should().Be(3.5);
        stats.OldestThought.Should().Be(oldest);
        stats.NewestThought.Should().Be(newest);
    }

    [Fact]
    public void Constructor_WithNullDates_AllowsNull()
    {
        var stats = new NeuroSymbolicStats(
            0, 0, 0,
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            0, 0.0, null, null);

        stats.OldestThought.Should().BeNull();
        stats.NewestThought.Should().BeNull();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var thoughtsByType = new Dictionary<string, int> { ["Obs"] = 10 };
        var relsByType = new Dictionary<string, int>();
        var resByType = new Dictionary<string, int>();

        var a = new NeuroSymbolicStats(10, 5, 3, thoughtsByType, relsByType, resByType, 2, 1.5, null, null);
        var b = new NeuroSymbolicStats(10, 5, 3, thoughtsByType, relsByType, resByType, 2, 1.5, null, null);

        a.Should().Be(b);
    }

    [Fact]
    public void WithExpression_ChangesTotalThoughts()
    {
        var stats = new NeuroSymbolicStats(
            10, 5, 3,
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            1, 2.0, null, null);

        var modified = stats with { TotalThoughts = 200 };

        modified.TotalThoughts.Should().Be(200);
        stats.TotalThoughts.Should().Be(10);
    }

    [Fact]
    public void WithExpression_ChangesAverageChainLength()
    {
        var stats = new NeuroSymbolicStats(
            50, 25, 10,
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            5, 3.0, null, null);

        var modified = stats with { AverageChainLength = 7.5 };

        modified.AverageChainLength.Should().Be(7.5);
    }
}
