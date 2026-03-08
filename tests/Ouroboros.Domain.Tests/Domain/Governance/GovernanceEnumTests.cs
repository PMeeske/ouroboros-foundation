using FluentAssertions;
using Ouroboros.Domain.Governance;
using Xunit;

namespace Ouroboros.Tests.Domain.Governance;

[Trait("Category", "Unit")]
public class GovernanceEnumTests
{
    [Theory]
    [InlineData(AlertSeverity.Info, 0)]
    [InlineData(AlertSeverity.Warning, 1)]
    [InlineData(AlertSeverity.Error, 2)]
    [InlineData(AlertSeverity.Critical, 3)]
    public void AlertSeverity_ValuesAreOrdered(AlertSeverity severity, int expected)
    {
        ((int)severity).Should().Be(expected);
    }

    [Fact]
    public void AlertSeverity_HasFourValues()
    {
        Enum.GetValues<AlertSeverity>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(PolicyAction.Log, 0)]
    [InlineData(PolicyAction.Alert, 1)]
    [InlineData(PolicyAction.Block, 2)]
    [InlineData(PolicyAction.RequireApproval, 3)]
    [InlineData(PolicyAction.Throttle, 4)]
    [InlineData(PolicyAction.Archive, 5)]
    [InlineData(PolicyAction.Compact, 6)]
    [InlineData(PolicyAction.Custom, 99)]
    public void PolicyAction_ValuesAreCorrect(PolicyAction action, int expected)
    {
        ((int)action).Should().Be(expected);
    }

    [Fact]
    public void PolicyAction_HasEightValues()
    {
        Enum.GetValues<PolicyAction>().Should().HaveCount(8);
    }

    [Theory]
    [InlineData(ViolationSeverity.Low, 0)]
    [InlineData(ViolationSeverity.Medium, 1)]
    [InlineData(ViolationSeverity.High, 2)]
    [InlineData(ViolationSeverity.Critical, 3)]
    public void ViolationSeverity_ValuesAreOrdered(ViolationSeverity severity, int expected)
    {
        ((int)severity).Should().Be(expected);
    }

    [Theory]
    [InlineData(ThresholdSeverity.Info, 0)]
    [InlineData(ThresholdSeverity.Warning, 1)]
    [InlineData(ThresholdSeverity.Error, 2)]
    [InlineData(ThresholdSeverity.Critical, 3)]
    public void ThresholdSeverity_ValuesAreOrdered(ThresholdSeverity severity, int expected)
    {
        ((int)severity).Should().Be(expected);
    }

    [Theory]
    [InlineData(ApprovalTimeoutAction.Block, 0)]
    [InlineData(ApprovalTimeoutAction.Escalate, 1)]
    [InlineData(ApprovalTimeoutAction.AutoApproveReduced, 2)]
    [InlineData(ApprovalTimeoutAction.AutoReject, 3)]
    public void ApprovalTimeoutAction_ValuesAreOrdered(ApprovalTimeoutAction action, int expected)
    {
        ((int)action).Should().Be(expected);
    }

    [Theory]
    [InlineData(ApprovalStatus.Pending, 0)]
    [InlineData(ApprovalStatus.Approved, 1)]
    [InlineData(ApprovalStatus.Rejected, 2)]
    [InlineData(ApprovalStatus.Expired, 3)]
    [InlineData(ApprovalStatus.Cancelled, 4)]
    public void ApprovalStatus_ValuesAreOrdered(ApprovalStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    [Theory]
    [InlineData(ApprovalDecision.Approve, 0)]
    [InlineData(ApprovalDecision.Reject, 1)]
    [InlineData(ApprovalDecision.RequestInfo, 2)]
    public void ApprovalDecision_ValuesAreCorrect(ApprovalDecision decision, int expected)
    {
        ((int)decision).Should().Be(expected);
    }

    [Theory]
    [InlineData(MaintenanceTaskType.Compaction, 0)]
    [InlineData(MaintenanceTaskType.Archiving, 1)]
    [InlineData(MaintenanceTaskType.AnomalyDetection, 2)]
    [InlineData(MaintenanceTaskType.Custom, 99)]
    public void MaintenanceTaskType_ValuesAreCorrect(MaintenanceTaskType type, int expected)
    {
        ((int)type).Should().Be(expected);
    }

    [Theory]
    [InlineData(MaintenanceStatus.Running, 0)]
    [InlineData(MaintenanceStatus.Completed, 1)]
    [InlineData(MaintenanceStatus.Failed, 2)]
    [InlineData(MaintenanceStatus.Cancelled, 3)]
    public void MaintenanceStatus_ValuesAreOrdered(MaintenanceStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }
}
