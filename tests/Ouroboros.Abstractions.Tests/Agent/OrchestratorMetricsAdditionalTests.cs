using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class OrchestratorMetricsAdditionalTests
{
    [Fact]
    public void WithCustomMetric_OverwritesExistingMetric()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("test")
            .WithCustomMetric("key", 1.0);

        // Act
        var updated = metrics.WithCustomMetric("key", 2.0);

        // Assert
        updated.GetCustomMetric("key").Should().Be(2.0);
        metrics.GetCustomMetric("key").Should().Be(1.0);
    }

    [Fact]
    public void GetCustomMetric_NoDefault_ReturnsZero()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("test");

        // Assert
        metrics.GetCustomMetric("nonexistent").Should().Be(0.0);
    }

    [Fact]
    public void RecordExecution_MixedSuccessAndFailure_CorrectCounts()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("test")
            .RecordExecution(100.0, true)
            .RecordExecution(200.0, false)
            .RecordExecution(300.0, true)
            .RecordExecution(400.0, false);

        // Assert
        metrics.TotalExecutions.Should().Be(4);
        metrics.SuccessfulExecutions.Should().Be(2);
        metrics.FailedExecutions.Should().Be(2);
        metrics.SuccessRate.Should().Be(0.5);
        metrics.AverageLatencyMs.Should().Be(250.0);
    }

    [Fact]
    public void Initial_LastExecuted_IsRecentUtcTime()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var metrics = OrchestratorMetrics.Initial("test");

        // Assert
        metrics.LastExecuted.Should().BeOnOrAfter(before);
        metrics.LastExecuted.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void RecordExecution_UpdatesLastExecuted()
    {
        // Arrange
        var metrics = OrchestratorMetrics.Initial("test");
        var before = DateTime.UtcNow;

        // Act
        var updated = metrics.RecordExecution(50.0, true);

        // Assert
        updated.LastExecuted.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void WithCustomMetric_MultipleDifferentKeys_AllPresent()
    {
        // Arrange & Act
        var metrics = OrchestratorMetrics.Initial("test")
            .WithCustomMetric("key1", 10.0)
            .WithCustomMetric("key2", 20.0)
            .WithCustomMetric("key3", 30.0);

        // Assert
        metrics.GetCustomMetric("key1").Should().Be(10.0);
        metrics.GetCustomMetric("key2").Should().Be(20.0);
        metrics.GetCustomMetric("key3").Should().Be(30.0);
    }
}
