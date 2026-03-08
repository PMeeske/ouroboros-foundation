using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class MetricsStoreStatisticsTests
{
    [Fact]
    public void MetricsStoreStatistics_AllPropertiesSet()
    {
        // Arrange
        var oldest = DateTime.UtcNow.AddDays(-30);
        var newest = DateTime.UtcNow;

        // Act
        var stats = new MetricsStoreStatistics(
            10, 500, 0.95, 45.0, oldest, newest);

        // Assert
        stats.TotalResources.Should().Be(10);
        stats.TotalExecutions.Should().Be(500);
        stats.OverallSuccessRate.Should().Be(0.95);
        stats.AverageLatencyMs.Should().Be(45.0);
        stats.OldestMetric.Should().Be(oldest);
        stats.NewestMetric.Should().Be(newest);
    }

    [Fact]
    public void MetricsStoreStatistics_DefaultOptionals_AreNull()
    {
        // Act
        var stats = new MetricsStoreStatistics(0, 0, 0.0, 0.0, null, null);

        // Assert
        stats.OldestMetric.Should().BeNull();
        stats.NewestMetric.Should().BeNull();
    }

    [Fact]
    public void MetricsStoreStatistics_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var ts = DateTime.UtcNow;
        var a = new MetricsStoreStatistics(5, 100, 0.9, 30.0, ts, ts);
        var b = new MetricsStoreStatistics(5, 100, 0.9, 30.0, ts, ts);

        // Assert
        a.Should().Be(b);
    }
}
