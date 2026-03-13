using Ouroboros.Abstractions.Monads;
using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

#region Enum Tests

[Trait("Category", "Unit")]
public class AlertSeverityTests
{
    [Fact]
    public void AlertSeverity_HasExpectedValues()
    {
        Enum.GetValues<AlertSeverity>().Should().HaveCount(4);
        ((int)AlertSeverity.Info).Should().Be(0);
        ((int)AlertSeverity.Warning).Should().Be(1);
        ((int)AlertSeverity.Error).Should().Be(2);
        ((int)AlertSeverity.Critical).Should().Be(3);
    }
}

[Trait("Category", "Unit")]
public class ApprovalDecisionTests
{
    [Fact]
    public void ApprovalDecision_HasExpectedValues()
    {
        Enum.GetValues<ApprovalDecision>().Should().HaveCount(3);
        ((int)ApprovalDecision.Approve).Should().Be(0);
        ((int)ApprovalDecision.Reject).Should().Be(1);
        ((int)ApprovalDecision.RequestInfo).Should().Be(2);
    }
}

[Trait("Category", "Unit")]
public class ApprovalStatusTests
{
    [Fact]
    public void ApprovalStatus_HasExpectedValues()
    {
        Enum.GetValues<ApprovalStatus>().Should().HaveCount(5);
        ((int)ApprovalStatus.Pending).Should().Be(0);
        ((int)ApprovalStatus.Approved).Should().Be(1);
        ((int)ApprovalStatus.Rejected).Should().Be(2);
        ((int)ApprovalStatus.Expired).Should().Be(3);
        ((int)ApprovalStatus.Cancelled).Should().Be(4);
    }
}

[Trait("Category", "Unit")]
public class ApprovalTimeoutActionTests
{
    [Fact]
    public void ApprovalTimeoutAction_HasExpectedValues()
    {
        Enum.GetValues<ApprovalTimeoutAction>().Should().HaveCount(4);
        ((int)ApprovalTimeoutAction.Block).Should().Be(0);
        ((int)ApprovalTimeoutAction.Escalate).Should().Be(1);
        ((int)ApprovalTimeoutAction.AutoApproveReduced).Should().Be(2);
        ((int)ApprovalTimeoutAction.AutoReject).Should().Be(3);
    }
}

[Trait("Category", "Unit")]
public class MaintenanceStatusTests
{
    [Fact]
    public void MaintenanceStatus_HasExpectedValues()
    {
        Enum.GetValues<MaintenanceStatus>().Should().HaveCount(4);
        ((int)MaintenanceStatus.Running).Should().Be(0);
        ((int)MaintenanceStatus.Completed).Should().Be(1);
        ((int)MaintenanceStatus.Failed).Should().Be(2);
        ((int)MaintenanceStatus.Cancelled).Should().Be(3);
    }
}

[Trait("Category", "Unit")]
public class MaintenanceTaskTypeTests
{
    [Fact]
    public void MaintenanceTaskType_HasExpectedValues()
    {
        Enum.GetValues<MaintenanceTaskType>().Should().HaveCount(4);
        ((int)MaintenanceTaskType.Compaction).Should().Be(0);
        ((int)MaintenanceTaskType.Archiving).Should().Be(1);
        ((int)MaintenanceTaskType.AnomalyDetection).Should().Be(2);
        ((int)MaintenanceTaskType.Custom).Should().Be(99);
    }
}

[Trait("Category", "Unit")]
public class PolicyActionTests
{
    [Fact]
    public void PolicyAction_HasExpectedValues()
    {
        Enum.GetValues<PolicyAction>().Should().HaveCount(8);
        ((int)PolicyAction.Log).Should().Be(0);
        ((int)PolicyAction.Alert).Should().Be(1);
        ((int)PolicyAction.Block).Should().Be(2);
        ((int)PolicyAction.RequireApproval).Should().Be(3);
        ((int)PolicyAction.Throttle).Should().Be(4);
        ((int)PolicyAction.Archive).Should().Be(5);
        ((int)PolicyAction.Compact).Should().Be(6);
        ((int)PolicyAction.Custom).Should().Be(99);
    }
}

[Trait("Category", "Unit")]
public class ThresholdSeverityTests
{
    [Fact]
    public void ThresholdSeverity_HasExpectedValues()
    {
        Enum.GetValues<ThresholdSeverity>().Should().HaveCount(4);
        ((int)ThresholdSeverity.Info).Should().Be(0);
        ((int)ThresholdSeverity.Warning).Should().Be(1);
        ((int)ThresholdSeverity.Error).Should().Be(2);
        ((int)ThresholdSeverity.Critical).Should().Be(3);
    }
}

[Trait("Category", "Unit")]
public class ViolationSeverityTests
{
    [Fact]
    public void ViolationSeverity_HasExpectedValues()
    {
        Enum.GetValues<ViolationSeverity>().Should().HaveCount(4);
        ((int)ViolationSeverity.Low).Should().Be(0);
        ((int)ViolationSeverity.Medium).Should().Be(1);
        ((int)ViolationSeverity.High).Should().Be(2);
        ((int)ViolationSeverity.Critical).Should().Be(3);
    }
}

#endregion

#region AnomalyDetectionResult Tests

[Trait("Category", "Unit")]
public class AnomalyDetectionResultTests
{
    [Fact]
    public void Construction_DefaultValues_AreCorrect()
    {
        var result = new AnomalyDetectionResult();

        result.Anomalies.Should().BeEmpty();
        result.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAnomalies_SetsValues()
    {
        var anomalies = new List<AnomalyAlert>
        {
            new() { MetricName = "cpu", Description = "high" },
            new() { MetricName = "mem", Description = "leak" }
        };

        var result = new AnomalyDetectionResult { Anomalies = anomalies };

        result.Anomalies.Should().HaveCount(2);
        result.Anomalies[0].MetricName.Should().Be("cpu");
        result.Anomalies[1].MetricName.Should().Be("mem");
    }
}

#endregion
