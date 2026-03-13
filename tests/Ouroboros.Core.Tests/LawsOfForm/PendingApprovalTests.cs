using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class PendingApprovalTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var call = new ToolCall("tool", "{}");
        var decision = AuditableDecision<ToolResult>.Uncertain("Uncertain", "Needs review");
        var queuedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var sut = new PendingApproval("q-123", call, decision, queuedAt);

        sut.QueueId.Should().Be("q-123");
        sut.Call.Should().Be(call);
        sut.OriginalDecision.Should().Be(decision);
        sut.QueuedAt.Should().Be(queuedAt);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var call = new ToolCall("tool", "{}", callId: "fixed-id", requestedAt: DateTime.UnixEpoch);
        var decision = AuditableDecision<ToolResult>.Uncertain("Uncertain", "reason");
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new PendingApproval("q1", call, decision, ts);
        var b = new PendingApproval("q1", call, decision, ts);

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentQueueId_AreNotEqual()
    {
        var call = new ToolCall("tool", "{}", callId: "fixed-id", requestedAt: DateTime.UnixEpoch);
        var decision = AuditableDecision<ToolResult>.Uncertain("Uncertain", "reason");
        var ts = DateTime.UnixEpoch;

        var a = new PendingApproval("q1", call, decision, ts);
        var b = new PendingApproval("q2", call, decision, ts);

        a.Should().NotBe(b);
    }
}
