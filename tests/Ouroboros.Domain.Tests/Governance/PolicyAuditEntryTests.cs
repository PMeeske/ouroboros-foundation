using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class PolicyAuditEntryTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var policy = Policy.Create("P", "D");
        var entry = new PolicyAuditEntry
        {
            Policy = policy,
            Action = "RegisterPolicy",
            Actor = "System"
        };

        entry.Id.Should().NotBeEmpty();
        entry.Policy.Should().Be(policy);
        entry.Action.Should().Be("RegisterPolicy");
        entry.Actor.Should().Be("System");
        entry.EvaluationResult.Should().BeNull();
        entry.ApprovalRequest.Should().BeNull();
        entry.Metadata.Should().BeEmpty();
        entry.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var id = Guid.NewGuid();
        var policy = Policy.Create("P", "D");
        var evaluation = new PolicyEvaluationResult { Policy = policy, IsCompliant = true };
        var timestamp = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        var entry = new PolicyAuditEntry
        {
            Id = id,
            Policy = policy,
            Action = "EvaluatePolicy",
            Actor = "Admin",
            EvaluationResult = evaluation,
            Timestamp = timestamp,
            Metadata = metadata
        };

        entry.Id.Should().Be(id);
        entry.Actor.Should().Be("Admin");
        entry.EvaluationResult.Should().Be(evaluation);
        entry.Timestamp.Should().Be(timestamp);
        entry.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var entry = new PolicyAuditEntry
        {
            Policy = Policy.Create("P", "D"),
            Action = "Test",
            Actor = "System"
        };

        var modified = entry with { Actor = "Admin" };

        modified.Actor.Should().Be("Admin");
        entry.Actor.Should().Be("System");
    }
}
