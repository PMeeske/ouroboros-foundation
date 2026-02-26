using Ouroboros.Abstractions.Agent.MetaAI;
using PerformanceMetricsMetaAI = Ouroboros.Abstractions.Agent.MetaAI.PerformanceMetrics;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class PerformanceMetricsMetaAITests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var customMetrics = new Dictionary<string, double> { ["throughput"] = 1000.0 };

        // Act
        var metrics = new PerformanceMetricsMetaAI(
            "resource-1", 100, 0.95, 50.0, DateTime.UtcNow, customMetrics);

        // Assert
        metrics.ResourceName.Should().Be("resource-1");
        metrics.ExecutionCount.Should().Be(100);
        metrics.SuccessRate.Should().Be(0.95);
        metrics.AverageLatencyMs.Should().Be(50.0);
        metrics.CustomMetrics.Should().ContainKey("throughput");
    }

    [Fact]
    public void Constructor_NullResourceName_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PerformanceMetricsMetaAI(
            null!, 0, 0, 0, DateTime.UtcNow, new Dictionary<string, double>());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullCustomMetrics_CreatesEmptyDictionary()
    {
        // Act
        var metrics = new PerformanceMetricsMetaAI(
            "resource", 0, 0, 0, DateTime.UtcNow, null!);

        // Assert
        metrics.CustomMetrics.Should().NotBeNull();
        metrics.CustomMetrics.Should().BeEmpty();
    }

    [Fact]
    public void Initial_CreatesMetricsWithZeroValues()
    {
        // Act
        var metrics = PerformanceMetricsMetaAI.Initial("new-resource");

        // Assert
        metrics.ResourceName.Should().Be("new-resource");
        metrics.ExecutionCount.Should().Be(0);
        metrics.SuccessRate.Should().Be(0.0);
        metrics.AverageLatencyMs.Should().Be(0.0);
        metrics.CustomMetrics.Should().BeEmpty();
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = PerformanceMetricsMetaAI.Initial("res");

        // Act
        var modified = original with { ExecutionCount = 50, SuccessRate = 0.8 };

        // Assert
        modified.ExecutionCount.Should().Be(50);
        modified.SuccessRate.Should().Be(0.8);
        modified.ResourceName.Should().Be("res");
    }
}
