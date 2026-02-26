using FluentAssertions;
using Ouroboros.Domain.Governance;
using Xunit;

namespace Ouroboros.Tests.Domain.Governance;

[Trait("Category", "Unit")]
public class GovernanceRecordTests
{
    [Fact]
    public void AnomalyAlert_Create_ShouldSetRequiredFields()
    {
        var alert = new AnomalyAlert
        {
            MetricName = "cpu_usage",
            Description = "CPU usage exceeds threshold"
        };

        alert.MetricName.Should().Be("cpu_usage");
        alert.Description.Should().Be("CPU usage exceeds threshold");
        alert.Id.Should().NotBeEmpty();
        alert.Severity.Should().Be(AlertSeverity.Warning);
        alert.IsResolved.Should().BeFalse();
        alert.ResolvedAt.Should().BeNull();
        alert.Resolution.Should().BeNull();
    }

    [Fact]
    public void AnomalyAlert_WithExpectedAndObservedValues()
    {
        var alert = new AnomalyAlert
        {
            MetricName = "memory",
            Description = "Memory spike",
            ExpectedValue = 80.0,
            ObservedValue = 95.0,
            Severity = AlertSeverity.Critical
        };

        alert.ExpectedValue.Should().Be(80.0);
        alert.ObservedValue.Should().Be(95.0);
        alert.Severity.Should().Be(AlertSeverity.Critical);
    }

    [Fact]
    public void ResourceQuota_IsExceeded_WhenCurrentExceedsMax_ShouldBeTrue()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "storage",
            MaxValue = 100.0,
            CurrentValue = 150.0,
            Unit = "GB"
        };

        quota.IsExceeded.Should().BeTrue();
    }

    [Fact]
    public void ResourceQuota_IsExceeded_WhenCurrentBelowMax_ShouldBeFalse()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "storage",
            MaxValue = 100.0,
            CurrentValue = 50.0,
            Unit = "GB"
        };

        quota.IsExceeded.Should().BeFalse();
    }

    [Fact]
    public void ResourceQuota_UtilizationPercent_ShouldCalculateCorrectly()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = 200.0,
            CurrentValue = 100.0,
            Unit = "cores"
        };

        quota.UtilizationPercent.Should().Be(50.0);
    }

    [Fact]
    public void ResourceQuota_UtilizationPercent_WhenMaxIsZero_ShouldBeZero()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = 0.0,
            CurrentValue = 100.0,
            Unit = "cores"
        };

        quota.UtilizationPercent.Should().Be(0.0);
    }

    [Fact]
    public void ResourceQuota_TimeWindow_ShouldBeNullByDefault()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "requests",
            MaxValue = 1000,
            Unit = "count"
        };

        quota.TimeWindow.Should().BeNull();
    }

    [Fact]
    public void Threshold_Create_ShouldSetRequiredFields()
    {
        var threshold = new Threshold
        {
            MetricName = "latency",
            UpperBound = 500.0,
            Action = PolicyAction.Alert
        };

        threshold.MetricName.Should().Be("latency");
        threshold.UpperBound.Should().Be(500.0);
        threshold.LowerBound.Should().BeNull();
        threshold.Severity.Should().Be(ThresholdSeverity.Warning);
    }

    [Fact]
    public void ApprovalGate_Create_ShouldSetDefaults()
    {
        var gate = new ApprovalGate
        {
            Id = Guid.NewGuid(),
            Name = "Production Deploy",
            Condition = "environment == production"
        };

        gate.RequiredApprovers.Should().BeEmpty();
        gate.MinimumApprovals.Should().Be(1);
        gate.ApprovalTimeout.Should().Be(TimeSpan.FromHours(24));
        gate.TimeoutAction.Should().Be(ApprovalTimeoutAction.Block);
    }

    [Fact]
    public void Approval_Create_ShouldSetRequiredFields()
    {
        var approval = new Approval
        {
            ApproverId = "user-123",
            Decision = ApprovalDecision.Approve,
            Comments = "Looks good"
        };

        approval.ApproverId.Should().Be("user-123");
        approval.Decision.Should().Be(ApprovalDecision.Approve);
        approval.Comments.Should().Be("Looks good");
    }

    [Fact]
    public void Policy_Create_FactoryMethod_ShouldSetDefaults()
    {
        var policy = Policy.Create("Test Policy", "A test policy");

        policy.Id.Should().NotBeEmpty();
        policy.Name.Should().Be("Test Policy");
        policy.Description.Should().Be("A test policy");
        policy.Priority.Should().Be(1.0);
        policy.IsActive.Should().BeTrue();
        policy.Rules.Should().BeEmpty();
        policy.Quotas.Should().BeEmpty();
        policy.Thresholds.Should().BeEmpty();
        policy.ApprovalGates.Should().BeEmpty();
    }

    [Fact]
    public void PolicyRule_Create_ShouldSetRequiredFields()
    {
        var rule = new PolicyRule
        {
            Id = Guid.NewGuid(),
            Name = "MaxTokens",
            Condition = "tokens > 4096",
            Action = PolicyAction.Block
        };

        rule.Name.Should().Be("MaxTokens");
        rule.Condition.Should().Be("tokens > 4096");
        rule.Action.Should().Be(PolicyAction.Block);
        rule.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void PolicyViolation_Create_ShouldSetRequiredFields()
    {
        var rule = new PolicyRule
        {
            Id = Guid.NewGuid(),
            Name = "MaxTokens",
            Condition = "tokens > 4096",
            Action = PolicyAction.Block
        };

        var violation = new PolicyViolation
        {
            Rule = rule,
            Severity = ViolationSeverity.High,
            Message = "Token limit exceeded",
            ActualValue = 5000,
            ExpectedValue = 4096,
            RecommendedAction = PolicyAction.Block
        };

        violation.Id.Should().NotBeEmpty();
        violation.Rule.Should().Be(rule);
        violation.Severity.Should().Be(ViolationSeverity.High);
        violation.Message.Should().Be("Token limit exceeded");
    }

    [Fact]
    public void PolicyEvaluationResult_Create_ShouldSetRequiredFields()
    {
        var policy = Policy.Create("Test", "Test");
        var result = new PolicyEvaluationResult
        {
            Policy = policy,
            IsCompliant = true
        };

        result.Policy.Should().Be(policy);
        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public void AnomalyDetectionResult_Default_ShouldHaveEmptyAnomalies()
    {
        var result = new AnomalyDetectionResult();
        result.Anomalies.Should().BeEmpty();
    }

    [Fact]
    public void PolicyEnforcementResult_Create_ShouldSetDefaults()
    {
        var evaluations = new List<PolicyEvaluationResult>();
        var result = new PolicyEnforcementResult
        {
            Evaluations = evaluations
        };

        result.ActionsRequired.Should().BeEmpty();
        result.ApprovalsRequired.Should().BeEmpty();
        result.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void ApprovalRequest_IsApproved_WithSufficientApprovals_ShouldBeTrue()
    {
        var gate = new ApprovalGate
        {
            Id = Guid.NewGuid(),
            Name = "Deploy Gate",
            Condition = "deploy",
            MinimumApprovals = 1
        };

        var approvals = new List<Approval>
        {
            new Approval { ApproverId = "user-1", Decision = ApprovalDecision.Approve }
        };

        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy to prod",
            Status = ApprovalStatus.Approved,
            Approvals = approvals,
            Deadline = DateTime.UtcNow.AddHours(24)
        };

        request.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void ApprovalRequest_IsApproved_WithInsufficientApprovals_ShouldBeFalse()
    {
        var gate = new ApprovalGate
        {
            Id = Guid.NewGuid(),
            Name = "Deploy Gate",
            Condition = "deploy",
            MinimumApprovals = 2
        };

        var approvals = new List<Approval>
        {
            new Approval { ApproverId = "user-1", Decision = ApprovalDecision.Approve }
        };

        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy to prod",
            Status = ApprovalStatus.Approved,
            Approvals = approvals,
            Deadline = DateTime.UtcNow.AddHours(24)
        };

        request.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void ApprovalRequest_IsExpired_WhenPastDeadline_ShouldBeTrue()
    {
        var gate = new ApprovalGate
        {
            Id = Guid.NewGuid(),
            Name = "Gate",
            Condition = "cond"
        };

        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "op",
            Deadline = DateTime.UtcNow.AddHours(-1)
        };

        request.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void PolicyAuditEntry_Create_ShouldSetRequiredFields()
    {
        var policy = Policy.Create("Audit Test", "Test audit");
        var entry = new PolicyAuditEntry
        {
            Policy = policy,
            Action = "evaluate",
            Actor = "system"
        };

        entry.Id.Should().NotBeEmpty();
        entry.Policy.Should().Be(policy);
        entry.Action.Should().Be("evaluate");
        entry.Actor.Should().Be("system");
        entry.EvaluationResult.Should().BeNull();
        entry.ApprovalRequest.Should().BeNull();
        entry.Metadata.Should().BeEmpty();
    }
}
