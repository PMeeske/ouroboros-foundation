using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class AnomalyAlertTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var alert = new AnomalyAlert
        {
            MetricName = "cpu_usage",
            Description = "CPU usage spike"
        };

        alert.MetricName.Should().Be("cpu_usage");
        alert.Description.Should().Be("CPU usage spike");
        alert.Id.Should().NotBeEmpty();
        alert.Severity.Should().Be(AlertSeverity.Warning);
        alert.IsResolved.Should().BeFalse();
        alert.ResolvedAt.Should().BeNull();
        alert.Resolution.Should().BeNull();
        alert.ExpectedValue.Should().BeNull();
        alert.ObservedValue.Should().BeNull();
        alert.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var id = Guid.NewGuid();
        var detectedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var resolvedAt = new DateTime(2025, 6, 1, 13, 0, 0, DateTimeKind.Utc);

        var alert = new AnomalyAlert
        {
            Id = id,
            MetricName = "memory_usage",
            Description = "Memory leak detected",
            Severity = AlertSeverity.Critical,
            ExpectedValue = 80.0,
            ObservedValue = 98.5,
            DetectedAt = detectedAt,
            IsResolved = true,
            ResolvedAt = resolvedAt,
            Resolution = "Restarted service"
        };

        alert.Id.Should().Be(id);
        alert.Severity.Should().Be(AlertSeverity.Critical);
        alert.ExpectedValue.Should().Be(80.0);
        alert.ObservedValue.Should().Be(98.5);
        alert.IsResolved.Should().BeTrue();
        alert.ResolvedAt.Should().Be(resolvedAt);
        alert.Resolution.Should().Be("Restarted service");
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var alert1 = new AnomalyAlert { Id = id, MetricName = "cpu", Description = "test", DetectedAt = timestamp };
        var alert2 = new AnomalyAlert { Id = id, MetricName = "cpu", Description = "test", DetectedAt = timestamp };

        alert1.Should().Be(alert2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var alert = new AnomalyAlert { MetricName = "cpu", Description = "spike" };
        var resolved = alert with { IsResolved = true, Resolution = "Fixed" };

        resolved.IsResolved.Should().BeTrue();
        resolved.Resolution.Should().Be("Fixed");
        alert.IsResolved.Should().BeFalse();
    }

    [Theory]
    [InlineData(AlertSeverity.Info)]
    [InlineData(AlertSeverity.Warning)]
    [InlineData(AlertSeverity.Error)]
    [InlineData(AlertSeverity.Critical)]
    public void Construction_AllSeverityLevels_Accepted(AlertSeverity severity)
    {
        var alert = new AnomalyAlert
        {
            MetricName = "test",
            Description = "test",
            Severity = severity
        };

        alert.Severity.Should().Be(severity);
    }
}
