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

#region AnomalyAlert Tests

[Trait("Category", "Unit")]
public class AnomalyAlertTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
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
        alert.MetricName.Should().Be("memory_usage");
        alert.Description.Should().Be("Memory leak detected");
        alert.Severity.Should().Be(AlertSeverity.Critical);
        alert.ExpectedValue.Should().Be(80.0);
        alert.ObservedValue.Should().Be(98.5);
        alert.DetectedAt.Should().Be(detectedAt);
        alert.IsResolved.Should().BeTrue();
        alert.ResolvedAt.Should().Be(resolvedAt);
        alert.Resolution.Should().Be("Restarted service");
    }

    [Fact]
    public void RecordEquality_WithSameValues_AreEqual()
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

        resolved.MetricName.Should().Be("cpu");
        resolved.IsResolved.Should().BeTrue();
        resolved.Resolution.Should().Be("Fixed");
        alert.IsResolved.Should().BeFalse();
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

#region Approval Tests

[Trait("Category", "Unit")]
public class ApprovalTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var approval = new Approval
        {
            ApproverId = "user-123",
            Decision = ApprovalDecision.Approve
        };

        approval.ApproverId.Should().Be("user-123");
        approval.Decision.Should().Be(ApprovalDecision.Approve);
        approval.Comments.Should().BeNull();
        approval.ApprovedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var timestamp = new DateTime(2025, 7, 1, 10, 0, 0, DateTimeKind.Utc);

        var approval = new Approval
        {
            ApproverId = "admin-1",
            Decision = ApprovalDecision.Reject,
            Comments = "Insufficient justification",
            ApprovedAt = timestamp
        };

        approval.ApproverId.Should().Be("admin-1");
        approval.Decision.Should().Be(ApprovalDecision.Reject);
        approval.Comments.Should().Be("Insufficient justification");
        approval.ApprovedAt.Should().Be(timestamp);
    }

    [Theory]
    [InlineData(ApprovalDecision.Approve)]
    [InlineData(ApprovalDecision.Reject)]
    [InlineData(ApprovalDecision.RequestInfo)]
    public void Construction_WithEachDecision_SetsDecision(ApprovalDecision decision)
    {
        var approval = new Approval { ApproverId = "user", Decision = decision };
        approval.Decision.Should().Be(decision);
    }
}

#endregion

#region ApprovalGate Tests

[Trait("Category", "Unit")]
public class ApprovalGateTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var id = Guid.NewGuid();
        var gate = new ApprovalGate
        {
            Id = id,
            Name = "Production Deploy",
            Condition = "environment == 'production'"
        };

        gate.Id.Should().Be(id);
        gate.Name.Should().Be("Production Deploy");
        gate.Condition.Should().Be("environment == 'production'");
        gate.RequiredApprovers.Should().BeEmpty();
        gate.MinimumApprovals.Should().Be(1);
        gate.ApprovalTimeout.Should().Be(TimeSpan.FromHours(24));
        gate.TimeoutAction.Should().Be(ApprovalTimeoutAction.Block);
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var id = Guid.NewGuid();
        var approvers = new List<string> { "admin", "lead", "security" };

        var gate = new ApprovalGate
        {
            Id = id,
            Name = "Security Review",
            Condition = "changes_security_config",
            RequiredApprovers = approvers,
            MinimumApprovals = 2,
            ApprovalTimeout = TimeSpan.FromHours(48),
            TimeoutAction = ApprovalTimeoutAction.Escalate
        };

        gate.RequiredApprovers.Should().BeEquivalentTo(approvers);
        gate.MinimumApprovals.Should().Be(2);
        gate.ApprovalTimeout.Should().Be(TimeSpan.FromHours(48));
        gate.TimeoutAction.Should().Be(ApprovalTimeoutAction.Escalate);
    }
}

#endregion

#region ApprovalRequest Tests

[Trait("Category", "Unit")]
public class ApprovalRequestTests
{
    private static ApprovalGate CreateGate(int minApprovals = 1) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Gate",
        Condition = "always",
        MinimumApprovals = minApprovals
    };

    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var gate = CreateGate();
        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy to prod"
        };

        request.Id.Should().NotBeEmpty();
        request.Gate.Should().Be(gate);
        request.OperationDescription.Should().Be("Deploy to prod");
        request.Context.Should().BeEmpty();
        request.Status.Should().Be(ApprovalStatus.Pending);
        request.Approvals.Should().BeEmpty();
        request.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void IsApproved_WhenStatusApprovedAndEnoughApprovals_ReturnsTrue()
    {
        var gate = CreateGate(minApprovals: 2);
        var approvals = new List<Approval>
        {
            new() { ApproverId = "user1", Decision = ApprovalDecision.Approve },
            new() { ApproverId = "user2", Decision = ApprovalDecision.Approve }
        };

        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy",
            Status = ApprovalStatus.Approved,
            Approvals = approvals
        };

        request.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void IsApproved_WhenStatusApprovedButInsufficientApprovals_ReturnsFalse()
    {
        var gate = CreateGate(minApprovals: 3);
        var approvals = new List<Approval>
        {
            new() { ApproverId = "user1", Decision = ApprovalDecision.Approve },
            new() { ApproverId = "user2", Decision = ApprovalDecision.Reject }
        };

        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy",
            Status = ApprovalStatus.Approved,
            Approvals = approvals
        };

        request.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void IsApproved_WhenStatusNotApproved_ReturnsFalse()
    {
        var gate = CreateGate(minApprovals: 1);
        var approvals = new List<Approval>
        {
            new() { ApproverId = "user1", Decision = ApprovalDecision.Approve }
        };

        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy",
            Status = ApprovalStatus.Pending,
            Approvals = approvals
        };

        request.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void IsApproved_WhenApprovalsIncludeRejects_OnlyCountsApprovals()
    {
        var gate = CreateGate(minApprovals: 2);
        var approvals = new List<Approval>
        {
            new() { ApproverId = "user1", Decision = ApprovalDecision.Approve },
            new() { ApproverId = "user2", Decision = ApprovalDecision.Reject },
            new() { ApproverId = "user3", Decision = ApprovalDecision.Approve }
        };

        var request = new ApprovalRequest
        {
            Gate = gate,
            OperationDescription = "Deploy",
            Status = ApprovalStatus.Approved,
            Approvals = approvals
        };

        request.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenDeadlinePassed_ReturnsTrue()
    {
        var request = new ApprovalRequest
        {
            Gate = CreateGate(),
            OperationDescription = "Deploy",
            Deadline = DateTime.UtcNow.AddHours(-1)
        };

        request.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenDeadlineNotPassed_ReturnsFalse()
    {
        var request = new ApprovalRequest
        {
            Gate = CreateGate(),
            OperationDescription = "Deploy",
            Deadline = DateTime.UtcNow.AddHours(1)
        };

        request.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Construction_WithContext_SetsValues()
    {
        var context = new Dictionary<string, object>
        {
            { "environment", "production" },
            { "risk_level", 5 }
        };

        var request = new ApprovalRequest
        {
            Gate = CreateGate(),
            OperationDescription = "Deploy",
            Context = context
        };

        request.Context.Should().HaveCount(2);
        request.Context["environment"].Should().Be("production");
    }
}

#endregion

#region ArchiveResult Tests

[Trait("Category", "Unit")]
public class ArchiveResultTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var result = new ArchiveResult { ArchiveLocation = "/archive/2025" };

        result.SnapshotsArchived.Should().Be(0);
        result.ArchiveLocation.Should().Be("/archive/2025");
        result.ArchivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var timestamp = new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = new ArchiveResult
        {
            SnapshotsArchived = 42,
            ArchiveLocation = "s3://bucket/archive",
            ArchivedAt = timestamp
        };

        result.SnapshotsArchived.Should().Be(42);
        result.ArchiveLocation.Should().Be("s3://bucket/archive");
        result.ArchivedAt.Should().Be(timestamp);
    }
}

#endregion

#region CompactionResult Tests

[Trait("Category", "Unit")]
public class CompactionResultTests
{
    [Fact]
    public void Construction_DefaultValues_AreCorrect()
    {
        var result = new CompactionResult();

        result.SnapshotsCompacted.Should().Be(0);
        result.BytesSaved.Should().Be(0);
        result.CompactedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var timestamp = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = new CompactionResult
        {
            SnapshotsCompacted = 100,
            BytesSaved = 1_073_741_824L,
            CompactedAt = timestamp
        };

        result.SnapshotsCompacted.Should().Be(100);
        result.BytesSaved.Should().Be(1_073_741_824L);
        result.CompactedAt.Should().Be(timestamp);
    }
}

#endregion

#region MaintenanceTask Tests

[Trait("Category", "Unit")]
public class MaintenanceTaskTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var id = Guid.NewGuid();
        var task = new MaintenanceTask
        {
            Id = id,
            Name = "Compact DB",
            Description = "Compact the database",
            TaskType = MaintenanceTaskType.Compaction,
            Schedule = TimeSpan.FromHours(6),
            Execute = _ => Task.FromResult(Result<object>.Success("done"))
        };

        task.Id.Should().Be(id);
        task.Name.Should().Be("Compact DB");
        task.Description.Should().Be("Compact the database");
        task.TaskType.Should().Be(MaintenanceTaskType.Compaction);
        task.Schedule.Should().Be(TimeSpan.FromHours(6));
        task.IsEnabled.Should().BeTrue();
        task.Execute.Should().NotBeNull();
    }

    [Fact]
    public void Construction_WithIsEnabledFalse_SetsValue()
    {
        var task = new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            Name = "Archive",
            Description = "Archive old data",
            TaskType = MaintenanceTaskType.Archiving,
            Schedule = TimeSpan.FromDays(1),
            IsEnabled = false,
            Execute = _ => Task.FromResult(Result<object>.Success("done"))
        };

        task.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Execute_WhenInvoked_ReturnsResult()
    {
        var task = new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Description = "Test task",
            TaskType = MaintenanceTaskType.Custom,
            Schedule = TimeSpan.FromMinutes(30),
            Execute = _ => Task.FromResult(Result<object>.Success((object)"completed"))
        };

        var result = await task.Execute(CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(MaintenanceTaskType.Compaction)]
    [InlineData(MaintenanceTaskType.Archiving)]
    [InlineData(MaintenanceTaskType.AnomalyDetection)]
    [InlineData(MaintenanceTaskType.Custom)]
    public void Construction_WithEachTaskType_SetsTaskType(MaintenanceTaskType taskType)
    {
        var task = new MaintenanceTask
        {
            Id = Guid.NewGuid(),
            Name = "Task",
            Description = "Desc",
            TaskType = taskType,
            Schedule = TimeSpan.FromHours(1),
            Execute = _ => Task.FromResult(Result<object>.Success("ok"))
        };

        task.TaskType.Should().Be(taskType);
    }
}

#endregion

#region MaintenanceExecution Tests

[Trait("Category", "Unit")]
public class MaintenanceExecutionTests
{
    private static MaintenanceTask CreateTask() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test",
        Description = "Test task",
        TaskType = MaintenanceTaskType.Custom,
        Schedule = TimeSpan.FromHours(1),
        Execute = _ => Task.FromResult(Result<object>.Success("ok"))
    };

    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var task = CreateTask();
        var execution = new MaintenanceExecution { Task = task };

        execution.Task.Should().Be(task);
        execution.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        execution.CompletedAt.Should().BeNull();
        execution.Status.Should().Be(MaintenanceStatus.Running);
        execution.ResultMessage.Should().BeNull();
        execution.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var task = CreateTask();
        var startedAt = new DateTime(2025, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        var completedAt = new DateTime(2025, 10, 1, 12, 5, 0, DateTimeKind.Utc);
        var metadata = new Dictionary<string, object> { { "rows_processed", 500 } };

        var execution = new MaintenanceExecution
        {
            Task = task,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            Status = MaintenanceStatus.Completed,
            ResultMessage = "Success",
            Metadata = metadata
        };

        execution.StartedAt.Should().Be(startedAt);
        execution.CompletedAt.Should().Be(completedAt);
        execution.Status.Should().Be(MaintenanceStatus.Completed);
        execution.ResultMessage.Should().Be("Success");
        execution.Metadata.Should().ContainKey("rows_processed");
    }

    [Theory]
    [InlineData(MaintenanceStatus.Running)]
    [InlineData(MaintenanceStatus.Completed)]
    [InlineData(MaintenanceStatus.Failed)]
    [InlineData(MaintenanceStatus.Cancelled)]
    public void Construction_WithEachStatus_SetsStatus(MaintenanceStatus status)
    {
        var execution = new MaintenanceExecution
        {
            Task = CreateTask(),
            Status = status
        };

        execution.Status.Should().Be(status);
    }
}

#endregion

#region Policy Tests

[Trait("Category", "Unit")]
public class PolicyTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var id = Guid.NewGuid();
        var policy = new Policy
        {
            Id = id,
            Name = "Security Policy",
            Description = "Enforce security standards"
        };

        policy.Id.Should().Be(id);
        policy.Name.Should().Be("Security Policy");
        policy.Description.Should().Be("Enforce security standards");
        policy.Priority.Should().Be(1.0);
        policy.IsActive.Should().BeTrue();
        policy.Rules.Should().BeEmpty();
        policy.Quotas.Should().BeEmpty();
        policy.Thresholds.Should().BeEmpty();
        policy.ApprovalGates.Should().BeEmpty();
        policy.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        policy.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var rules = new List<PolicyRule>
        {
            new() { Id = Guid.NewGuid(), Name = "Rule1", Condition = "cond1", Action = PolicyAction.Log }
        };
        var quotas = new List<ResourceQuota>
        {
            new() { ResourceName = "cpu", MaxValue = 100, Unit = "%" }
        };
        var thresholds = new List<Threshold>
        {
            new() { MetricName = "latency", Action = PolicyAction.Alert, UpperBound = 500 }
        };
        var gates = new List<ApprovalGate>
        {
            new() { Id = Guid.NewGuid(), Name = "Gate1", Condition = "cond1" }
        };

        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "Full Policy",
            Description = "Full",
            Priority = 10.0,
            IsActive = false,
            Rules = rules,
            Quotas = quotas,
            Thresholds = thresholds,
            ApprovalGates = gates
        };

        policy.Priority.Should().Be(10.0);
        policy.IsActive.Should().BeFalse();
        policy.Rules.Should().HaveCount(1);
        policy.Quotas.Should().HaveCount(1);
        policy.Thresholds.Should().HaveCount(1);
        policy.ApprovalGates.Should().HaveCount(1);
    }

    [Fact]
    public void Create_FactoryMethod_SetsNameAndDescription()
    {
        var policy = Policy.Create("Test Policy", "A test");

        policy.Id.Should().NotBeEmpty();
        policy.Name.Should().Be("Test Policy");
        policy.Description.Should().Be("A test");
        policy.Priority.Should().Be(1.0);
        policy.IsActive.Should().BeTrue();
        policy.Rules.Should().BeEmpty();
        policy.Quotas.Should().BeEmpty();
        policy.Thresholds.Should().BeEmpty();
        policy.ApprovalGates.Should().BeEmpty();
        policy.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        policy.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_FactoryMethod_GeneratesUniqueIds()
    {
        var policy1 = Policy.Create("A", "a");
        var policy2 = Policy.Create("B", "b");

        policy1.Id.Should().NotBe(policy2.Id);
    }
}

#endregion

#region PolicyRule Tests

[Trait("Category", "Unit")]
public class PolicyRuleTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var id = Guid.NewGuid();
        var rule = new PolicyRule
        {
            Id = id,
            Name = "Max CPU",
            Condition = "cpu_usage > 90",
            Action = PolicyAction.Block
        };

        rule.Id.Should().Be(id);
        rule.Name.Should().Be("Max CPU");
        rule.Condition.Should().Be("cpu_usage > 90");
        rule.Action.Should().Be(PolicyAction.Block);
        rule.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Construction_WithMetadata_SetsValues()
    {
        var metadata = new Dictionary<string, object>
        {
            { "category", "performance" },
            { "version", 2 }
        };

        var rule = new PolicyRule
        {
            Id = Guid.NewGuid(),
            Name = "Rule",
            Condition = "cond",
            Action = PolicyAction.Alert,
            Metadata = metadata
        };

        rule.Metadata.Should().HaveCount(2);
        rule.Metadata["category"].Should().Be("performance");
    }
}

#endregion

#region PolicyViolation Tests

[Trait("Category", "Unit")]
public class PolicyViolationTests
{
    private static PolicyRule CreateRule() => new()
    {
        Id = Guid.NewGuid(),
        Name = "TestRule",
        Condition = "cond",
        Action = PolicyAction.Block
    };

    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var rule = CreateRule();
        var violation = new PolicyViolation
        {
            Rule = rule,
            Message = "Threshold exceeded",
            RecommendedAction = PolicyAction.Alert
        };

        violation.Id.Should().NotBeEmpty();
        violation.Rule.Should().Be(rule);
        violation.Severity.Should().Be(ViolationSeverity.Low);
        violation.Message.Should().Be("Threshold exceeded");
        violation.ActualValue.Should().BeNull();
        violation.ExpectedValue.Should().BeNull();
        violation.RecommendedAction.Should().Be(PolicyAction.Alert);
        violation.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var rule = CreateRule();
        var id = Guid.NewGuid();
        var timestamp = new DateTime(2025, 11, 1, 0, 0, 0, DateTimeKind.Utc);

        var violation = new PolicyViolation
        {
            Id = id,
            Rule = rule,
            Severity = ViolationSeverity.Critical,
            Message = "CPU exceeded",
            ActualValue = 99.5,
            ExpectedValue = 90.0,
            RecommendedAction = PolicyAction.Block,
            DetectedAt = timestamp
        };

        violation.Id.Should().Be(id);
        violation.Severity.Should().Be(ViolationSeverity.Critical);
        violation.ActualValue.Should().Be(99.5);
        violation.ExpectedValue.Should().Be(90.0);
        violation.DetectedAt.Should().Be(timestamp);
    }
}

#endregion

#region PolicyEvaluationResult Tests

[Trait("Category", "Unit")]
public class PolicyEvaluationResultTests
{
    private static Policy CreatePolicy() => Policy.Create("Test", "Test policy");

    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var policy = CreatePolicy();
        var result = new PolicyEvaluationResult
        {
            Policy = policy,
            IsCompliant = true
        };

        result.Policy.Should().Be(policy);
        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
        result.EvaluatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Context.Should().BeEmpty();
    }

    [Fact]
    public void Construction_WithViolations_SetsValues()
    {
        var policy = CreatePolicy();
        var rule = new PolicyRule
        {
            Id = Guid.NewGuid(),
            Name = "Rule1",
            Condition = "cond",
            Action = PolicyAction.Block
        };
        var violations = new List<PolicyViolation>
        {
            new() { Rule = rule, Message = "Violation 1", RecommendedAction = PolicyAction.Alert },
            new() { Rule = rule, Message = "Violation 2", RecommendedAction = PolicyAction.Block }
        };
        var context = new Dictionary<string, object> { { "source", "test" } };

        var result = new PolicyEvaluationResult
        {
            Policy = policy,
            IsCompliant = false,
            Violations = violations,
            Context = context
        };

        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().HaveCount(2);
        result.Context.Should().ContainKey("source");
    }
}

#endregion

#region PolicyEnforcementResult Tests

[Trait("Category", "Unit")]
public class PolicyEnforcementResultTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var evaluations = new List<PolicyEvaluationResult>
        {
            new() { Policy = Policy.Create("P1", "Desc"), IsCompliant = true }
        };

        var result = new PolicyEnforcementResult { Evaluations = evaluations };

        result.Evaluations.Should().HaveCount(1);
        result.ActionsRequired.Should().BeEmpty();
        result.ApprovalsRequired.Should().BeEmpty();
        result.IsBlocked.Should().BeFalse();
        result.EnforcedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var policy = Policy.Create("P1", "Desc");
        var evaluations = new List<PolicyEvaluationResult>
        {
            new() { Policy = policy, IsCompliant = false }
        };
        var actions = new List<PolicyAction> { PolicyAction.Block, PolicyAction.Alert };
        var gate = new ApprovalGate { Id = Guid.NewGuid(), Name = "Gate", Condition = "cond" };
        var approvalRequests = new List<ApprovalRequest>
        {
            new() { Gate = gate, OperationDescription = "Deploy" }
        };

        var result = new PolicyEnforcementResult
        {
            Evaluations = evaluations,
            ActionsRequired = actions,
            ApprovalsRequired = approvalRequests,
            IsBlocked = true
        };

        result.ActionsRequired.Should().HaveCount(2);
        result.ApprovalsRequired.Should().HaveCount(1);
        result.IsBlocked.Should().BeTrue();
    }
}

#endregion

#region PolicyAuditEntry Tests

[Trait("Category", "Unit")]
public class PolicyAuditEntryTests
{
    private static Policy CreatePolicy() => Policy.Create("AuditPolicy", "For audit");

    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var policy = CreatePolicy();
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
        entry.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entry.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var policy = CreatePolicy();
        var evalResult = new PolicyEvaluationResult { Policy = policy, IsCompliant = true };
        var gate = new ApprovalGate { Id = Guid.NewGuid(), Name = "Gate", Condition = "cond" };
        var approvalReq = new ApprovalRequest { Gate = gate, OperationDescription = "Op" };
        var metadata = new Dictionary<string, object> { { "ip", "127.0.0.1" } };
        var id = Guid.NewGuid();
        var timestamp = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);

        var entry = new PolicyAuditEntry
        {
            Id = id,
            Policy = policy,
            Action = "enforce",
            Actor = "admin-user",
            EvaluationResult = evalResult,
            ApprovalRequest = approvalReq,
            Timestamp = timestamp,
            Metadata = metadata
        };

        entry.Id.Should().Be(id);
        entry.Actor.Should().Be("admin-user");
        entry.EvaluationResult.Should().Be(evalResult);
        entry.ApprovalRequest.Should().Be(approvalReq);
        entry.Timestamp.Should().Be(timestamp);
        entry.Metadata.Should().ContainKey("ip");
    }
}

#endregion

#region PolicySimulationResult Tests

[Trait("Category", "Unit")]
public class PolicySimulationResultTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var policy = Policy.Create("SimPolicy", "Sim");
        var evalResult = new PolicyEvaluationResult { Policy = policy, IsCompliant = true };

        var result = new PolicySimulationResult
        {
            Policy = policy,
            EvaluationResult = evalResult
        };

        result.Policy.Should().Be(policy);
        result.EvaluationResult.Should().Be(evalResult);
        result.WouldBlock.Should().BeFalse();
        result.RequiredApprovals.Should().BeEmpty();
        result.SimulatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var policy = Policy.Create("SimPolicy", "Sim");
        var evalResult = new PolicyEvaluationResult { Policy = policy, IsCompliant = false };
        var gates = new List<ApprovalGate>
        {
            new() { Id = Guid.NewGuid(), Name = "Gate1", Condition = "cond1" },
            new() { Id = Guid.NewGuid(), Name = "Gate2", Condition = "cond2" }
        };

        var result = new PolicySimulationResult
        {
            Policy = policy,
            EvaluationResult = evalResult,
            WouldBlock = true,
            RequiredApprovals = gates
        };

        result.WouldBlock.Should().BeTrue();
        result.RequiredApprovals.Should().HaveCount(2);
    }
}

#endregion

#region ResourceQuota Tests

[Trait("Category", "Unit")]
public class ResourceQuotaTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = 100.0,
            Unit = "%"
        };

        quota.ResourceName.Should().Be("cpu");
        quota.MaxValue.Should().Be(100.0);
        quota.CurrentValue.Should().Be(0.0);
        quota.Unit.Should().Be("%");
        quota.TimeWindow.Should().BeNull();
    }

    [Fact]
    public void IsExceeded_WhenCurrentValueExceedsMax_ReturnsTrue()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "memory",
            MaxValue = 8192,
            CurrentValue = 9000,
            Unit = "MB"
        };

        quota.IsExceeded.Should().BeTrue();
    }

    [Fact]
    public void IsExceeded_WhenCurrentValueEqualsMax_ReturnsFalse()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "memory",
            MaxValue = 8192,
            CurrentValue = 8192,
            Unit = "MB"
        };

        quota.IsExceeded.Should().BeFalse();
    }

    [Fact]
    public void IsExceeded_WhenCurrentValueBelowMax_ReturnsFalse()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "memory",
            MaxValue = 8192,
            CurrentValue = 4096,
            Unit = "MB"
        };

        quota.IsExceeded.Should().BeFalse();
    }

    [Fact]
    public void UtilizationPercent_CalculatesCorrectly()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = 200.0,
            CurrentValue = 50.0,
            Unit = "cores"
        };

        quota.UtilizationPercent.Should().Be(25.0);
    }

    [Fact]
    public void UtilizationPercent_WhenMaxIsZero_ReturnsZero()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "special",
            MaxValue = 0.0,
            CurrentValue = 10.0,
            Unit = "units"
        };

        quota.UtilizationPercent.Should().Be(0.0);
    }

    [Fact]
    public void UtilizationPercent_WhenFullyUtilized_Returns100()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "storage",
            MaxValue = 500.0,
            CurrentValue = 500.0,
            Unit = "GB"
        };

        quota.UtilizationPercent.Should().Be(100.0);
    }

    [Fact]
    public void UtilizationPercent_WhenOverUtilized_ExceedsHundred()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "requests",
            MaxValue = 100.0,
            CurrentValue = 150.0,
            Unit = "req/s"
        };

        quota.UtilizationPercent.Should().Be(150.0);
    }

    [Fact]
    public void Construction_WithTimeWindow_SetsValue()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "requests_per_hour",
            MaxValue = 1000,
            Unit = "requests",
            TimeWindow = TimeSpan.FromHours(1)
        };

        quota.TimeWindow.Should().Be(TimeSpan.FromHours(1));
    }
}

#endregion

#region Threshold Tests

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
    public void Construction_WithAllProperties_SetsValues()
    {
        var threshold = new Threshold
        {
            MetricName = "response_time",
            LowerBound = 10.0,
            UpperBound = 500.0,
            Action = PolicyAction.Block,
            Severity = ThresholdSeverity.Critical
        };

        threshold.LowerBound.Should().Be(10.0);
        threshold.UpperBound.Should().Be(500.0);
        threshold.Action.Should().Be(PolicyAction.Block);
        threshold.Severity.Should().Be(ThresholdSeverity.Critical);
    }

    [Fact]
    public void Construction_WithOnlyUpperBound_SetsValue()
    {
        var threshold = new Threshold
        {
            MetricName = "error_rate",
            UpperBound = 5.0,
            Action = PolicyAction.Alert,
            Severity = ThresholdSeverity.Error
        };

        threshold.LowerBound.Should().BeNull();
        threshold.UpperBound.Should().Be(5.0);
    }

    [Fact]
    public void Construction_WithOnlyLowerBound_SetsValue()
    {
        var threshold = new Threshold
        {
            MetricName = "availability",
            LowerBound = 99.9,
            Action = PolicyAction.RequireApproval,
            Severity = ThresholdSeverity.Info
        };

        threshold.LowerBound.Should().Be(99.9);
        threshold.UpperBound.Should().BeNull();
    }
}

#endregion
