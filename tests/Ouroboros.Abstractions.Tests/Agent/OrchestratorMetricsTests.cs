using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class OrchestratorMetricsTests
{
    [Fact]
    public void Initial_CreatesMetricsWithZeroValues()
    {
        // Act
        var metrics = OrchestratorMetrics.Initial("test-orchestrator");

        // Assert
        metrics.OrchestratorName.Should().Be("test-orchestrator");
        metrics.TotalExecutions.Should().Be(0);
        metrics.SuccessfulExecutions.Should().Be(0);
        metrics.FailedExecutions.Should().Be(0);
        metrics.AverageLatencyMs.Should().Be(0.0);
        metrics.SuccessRate.Should().Be(0.0);
        metrics.CustomMetrics.Should().BeEmpty();
    }

    [Fact]
    public void CalculatedSuccessRate_NoExecutions_ReturnsZero()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("test");

        // Assert
        metrics.CalculatedSuccessRate.Should().Be(0.0);
    }

    [Fact]
    public void CalculatedSuccessRate_WithExecutions_ReturnsCorrectRate()
    {
        // Arrange
        var metrics = new OrchestratorMetrics(
            "test", 10, 8, 2, 100.0, 0.8,
            DateTime.UtcNow, new Dictionary<string, double>());

        // Assert
        metrics.CalculatedSuccessRate.Should().Be(0.8);
    }

    [Fact]
    public void RecordExecution_Success_IncrementsCounters()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("test");

        // Act
        var updated = metrics.RecordExecution(100.0, true);

        // Assert
        updated.TotalExecutions.Should().Be(1);
        updated.SuccessfulExecutions.Should().Be(1);
        updated.FailedExecutions.Should().Be(0);
        updated.SuccessRate.Should().Be(1.0);
        updated.AverageLatencyMs.Should().Be(100.0);
    }

    [Fact]
    public void RecordExecution_Failure_IncrementsCounters()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("test");

        // Act
        var updated = metrics.RecordExecution(50.0, false);

        // Assert
        updated.TotalExecutions.Should().Be(1);
        updated.SuccessfulExecutions.Should().Be(0);
        updated.FailedExecutions.Should().Be(1);
        updated.SuccessRate.Should().Be(0.0);
    }

    [Fact]
    public void RecordExecution_Multiple_AveragesLatency()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("test");

        // Act
        var updated = metrics
            .RecordExecution(100.0, true)
            .RecordExecution(200.0, true);

        // Assert
        updated.TotalExecutions.Should().Be(2);
        updated.AverageLatencyMs.Should().Be(150.0);
    }

    [Fact]
    public void GetCustomMetric_ExistingKey_ReturnsValue()
    {
        // Arrange
        var metrics = new OrchestratorMetrics(
            "test", 0, 0, 0, 0, 0, DateTime.UtcNow,
            new Dictionary<string, double> { ["key1"] = 42.0 });

        // Assert
        metrics.GetCustomMetric("key1").Should().Be(42.0);
    }

    [Fact]
    public void GetCustomMetric_NonExistingKey_ReturnsDefault()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("test");

        // Assert
        metrics.GetCustomMetric("nonexistent", 99.0).Should().Be(99.0);
    }

    [Fact]
    public void WithCustomMetric_AddsOrUpdatesMetric()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("test");

        // Act
        var updated = metrics.WithCustomMetric("throughput", 1000.0);

        // Assert
        updated.GetCustomMetric("throughput").Should().Be(1000.0);
        metrics.CustomMetrics.Should().NotContainKey("throughput"); // original unchanged
    }

    [Fact]
    public void RecordExecution_DoesNotMutateOriginal()
    {
        // Arrange
        var original = OrchestratorMetrics.Initial("test");

        // Act
        var updated = original.RecordExecution(100.0, true);

        // Assert
        original.TotalExecutions.Should().Be(0);
        updated.TotalExecutions.Should().Be(1);
    }
}
