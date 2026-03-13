using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class ApprovalGateTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var gate = new ApprovalGate
        {
            Id = Guid.NewGuid(),
            Name = "SecurityGate",
            Condition = "high-risk"
        };

        gate.Name.Should().Be("SecurityGate");
        gate.Condition.Should().Be("high-risk");
        gate.MinimumApprovals.Should().Be(1);
        gate.ApprovalTimeout.Should().Be(TimeSpan.FromHours(24));
        gate.TimeoutAction.Should().Be(ApprovalTimeoutAction.Block);
        gate.RequiredApprovers.Should().BeEmpty();
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var id = Guid.NewGuid();
        var approvers = new[] { "admin", "security-team" };

        var gate = new ApprovalGate
        {
            Id = id,
            Name = "CriticalGate",
            Condition = "critical-change",
            RequiredApprovers = approvers,
            MinimumApprovals = 2,
            ApprovalTimeout = TimeSpan.FromHours(4),
            TimeoutAction = ApprovalTimeoutAction.Escalate
        };

        gate.Id.Should().Be(id);
        gate.RequiredApprovers.Should().HaveCount(2);
        gate.MinimumApprovals.Should().Be(2);
        gate.ApprovalTimeout.Should().Be(TimeSpan.FromHours(4));
        gate.TimeoutAction.Should().Be(ApprovalTimeoutAction.Escalate);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var gate1 = new ApprovalGate { Id = id, Name = "Gate", Condition = "cond" };
        var gate2 = new ApprovalGate { Id = id, Name = "Gate", Condition = "cond" };

        gate1.Should().Be(gate2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var gate = new ApprovalGate { Id = Guid.NewGuid(), Name = "Gate", Condition = "cond" };
        var modified = gate with { MinimumApprovals = 3 };

        modified.MinimumApprovals.Should().Be(3);
        modified.Name.Should().Be("Gate");
        gate.MinimumApprovals.Should().Be(1);
    }
}
