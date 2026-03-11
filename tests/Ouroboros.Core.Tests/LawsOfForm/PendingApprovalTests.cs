// <copyright file="PendingApprovalTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="PendingApproval"/> record.
/// </summary>
[Trait("Category", "Unit")]
public class PendingApprovalTests
{
    private static ToolCall CreateToolCall(string name = "tool") => new(name, "{}");

    private static AuditableDecision<ToolResult> CreateDecision() =>
        AuditableDecision<ToolResult>.Uncertain("uncertain", "needs review");

    // --- Constructor ---

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var call = CreateToolCall("myTool");
        var decision = CreateDecision();
        var queuedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var pending = new PendingApproval("q-123", call, decision, queuedAt);

        pending.QueueId.Should().Be("q-123");
        pending.Call.Should().Be(call);
        pending.OriginalDecision.Should().Be(decision);
        pending.QueuedAt.Should().Be(queuedAt);
    }

    [Fact]
    public void Constructor_PreservesToolCallDetails()
    {
        var call = CreateToolCall("specificTool");
        var decision = CreateDecision();

        var pending = new PendingApproval("id", call, decision, DateTime.UtcNow);

        pending.Call.ToolName.Should().Be("specificTool");
    }

    [Fact]
    public void Constructor_PreservesOriginalDecisionState()
    {
        var call = CreateToolCall();
        var decision = CreateDecision();

        var pending = new PendingApproval("id", call, decision, DateTime.UtcNow);

        pending.OriginalDecision.Certainty.Should().Be(Form.Imaginary);
    }

    // --- Record Equality ---

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var call = CreateToolCall();
        var decision = CreateDecision();
        var queuedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var p1 = new PendingApproval("q1", call, decision, queuedAt);
        var p2 = new PendingApproval("q1", call, decision, queuedAt);

        p1.Should().Be(p2);
    }

    [Fact]
    public void RecordEquality_DifferentQueueId_AreNotEqual()
    {
        var call = CreateToolCall();
        var decision = CreateDecision();
        var queuedAt = DateTime.UtcNow;

        var p1 = new PendingApproval("q1", call, decision, queuedAt);
        var p2 = new PendingApproval("q2", call, decision, queuedAt);

        p1.Should().NotBe(p2);
    }

    [Fact]
    public void RecordEquality_DifferentCall_AreNotEqual()
    {
        var decision = CreateDecision();
        var queuedAt = DateTime.UtcNow;

        var p1 = new PendingApproval("q1", CreateToolCall("tool1"), decision, queuedAt);
        var p2 = new PendingApproval("q1", CreateToolCall("tool2"), decision, queuedAt);

        p1.Should().NotBe(p2);
    }

    [Fact]
    public void RecordEquality_DifferentQueuedAt_AreNotEqual()
    {
        var call = CreateToolCall();
        var decision = CreateDecision();

        var p1 = new PendingApproval("q1", call, decision, new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var p2 = new PendingApproval("q1", call, decision, new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc));

        p1.Should().NotBe(p2);
    }

    // --- With expression (record) ---

    [Fact]
    public void WithExpression_CanCreateModifiedCopy()
    {
        var call = CreateToolCall();
        var decision = CreateDecision();
        var original = new PendingApproval("q1", call, decision, DateTime.UtcNow);

        var modified = original with { QueueId = "q2" };

        modified.QueueId.Should().Be("q2");
        modified.Call.Should().Be(call);
        original.QueueId.Should().Be("q1");
    }
}
