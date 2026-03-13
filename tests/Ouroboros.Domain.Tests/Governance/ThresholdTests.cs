using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class ThresholdTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var threshold = new Threshold
        {
            MetricName = "latency_ms",
            Action = PolicyAction.Alert
        };

        threshold.MetricName.Should().Be("latency_ms");
        threshold.Action.Should().Be(PolicyAction.Alert);
        threshold.LowerBound.Should().BeNull();
        threshold.UpperBound.Should().BeNull();
        threshold.Severity.Should().Be(ThresholdSeverity.Warning);
    }

    [Fact]
    public void Construction_WithBounds_SetsValues()
    {
        var threshold = new Threshold
        {
            MetricName = "temperature",
            LowerBound = 10.0,
            UpperBound = 90.0,
            Action = PolicyAction.Block,
            Severity = ThresholdSeverity.Critical
        };

        threshold.LowerBound.Should().Be(10.0);
        threshold.UpperBound.Should().Be(90.0);
        threshold.Severity.Should().Be(ThresholdSeverity.Critical);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var t1 = new Threshold { MetricName = "cpu", Action = PolicyAction.Log, UpperBound = 90 };
        var t2 = new Threshold { MetricName = "cpu", Action = PolicyAction.Log, UpperBound = 90 };

        t1.Should().Be(t2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var threshold = new Threshold { MetricName = "cpu", Action = PolicyAction.Log };
        var modified = threshold with { UpperBound = 95.0 };

        modified.UpperBound.Should().Be(95.0);
        threshold.UpperBound.Should().BeNull();
    }
}
