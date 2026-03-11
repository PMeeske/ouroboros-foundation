using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class PerformanceMetricsAdditionalTests
{
    [Fact]
    public void Initial_NullResourceName_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => PerformanceMetrics.Initial(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Initial_CreatesMetricsWithZeroValues()
    {
        // Act
        var metrics = PerformanceMetrics.Initial("my-resource");

        // Assert
        metrics.ResourceName.Should().Be("my-resource");
        metrics.ExecutionCount.Should().Be(0);
        metrics.AverageLatencyMs.Should().Be(0.0);
        metrics.SuccessRate.Should().Be(0.0);
        metrics.CustomMetrics.Should().BeEmpty();
    }

    [Fact]
    public void Initial_LastUsed_IsRecentUtcTime()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var metrics = PerformanceMetrics.Initial("resource");

        // Assert
        metrics.LastUsed.Should().BeOnOrAfter(before);
        metrics.LastUsed.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var ts = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var customMetrics = new Dictionary<string, double>();

        var a = new PerformanceMetrics("res", 10, 50.0, 0.9, ts, customMetrics);
        var b = new PerformanceMetrics("res", 10, 50.0, 0.9, ts, customMetrics);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = PerformanceMetrics.Initial("resource");

        // Act
        var modified = original with { ExecutionCount = 50, SuccessRate = 0.95 };

        // Assert
        modified.ExecutionCount.Should().Be(50);
        modified.SuccessRate.Should().Be(0.95);
        modified.ResourceName.Should().Be("resource");
    }
}
