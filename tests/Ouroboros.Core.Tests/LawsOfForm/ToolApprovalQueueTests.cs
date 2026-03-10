// <copyright file="ToolApprovalQueueTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="ToolApprovalQueue"/> class.
/// </summary>
[Trait("Category", "Unit")]
public class ToolApprovalQueueTests
{
    private static ToolCall CreateToolCall(string name = "tool") => new(name, "{}");

    private static AuditableDecision<ToolResult> CreateUncertainDecision() =>
        AuditableDecision<ToolResult>.Uncertain("uncertain", "needs review");

    // --- Enqueue ---

    [Fact]
    public void Enqueue_ReturnsNonEmptyQueueId()
    {
        var queue = new ToolApprovalQueue();

        var id = queue.Enqueue(CreateToolCall(), CreateUncertainDecision());

        id.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(id, out _).Should().BeTrue();
    }

    [Fact]
    public void Enqueue_IncrementsPendingCount()
    {
        var queue = new ToolApprovalQueue();

        queue.Enqueue(CreateToolCall(), CreateUncertainDecision());
        queue.Enqueue(CreateToolCall(), CreateUncertainDecision());

        queue.PendingCount.Should().Be(2);
    }

    [Fact]
    public void Enqueue_NullCall_ThrowsArgumentNullException()
    {
        var queue = new ToolApprovalQueue();

        var act = () => queue.Enqueue(null!, CreateUncertainDecision());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Enqueue_NullDecision_ThrowsArgumentNullException()
    {
        var queue = new ToolApprovalQueue();

        var act = () => queue.Enqueue(CreateToolCall(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // --- GetPending (all) ---

    [Fact]
    public async Task GetPending_ReturnsAllPendingItems()
    {
        var queue = new ToolApprovalQueue();
        queue.Enqueue(CreateToolCall("a"), CreateUncertainDecision());
        queue.Enqueue(CreateToolCall("b"), CreateUncertainDecision());

        var pending = await queue.GetPending();

        pending.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPending_ReturnsOrderedByQueuedAt()
    {
        var queue = new ToolApprovalQueue();
        queue.Enqueue(CreateToolCall("first"), CreateUncertainDecision());
        queue.Enqueue(CreateToolCall("second"), CreateUncertainDecision());

        var pending = await queue.GetPending();

        pending[0].Call.ToolName.Should().Be("first");
        pending[1].Call.ToolName.Should().Be("second");
    }

    // --- GetPending (by ID) ---

    [Fact]
    public void GetPending_ValidId_ReturnsPendingApproval()
    {
        var queue = new ToolApprovalQueue();
        var call = CreateToolCall();
        var id = queue.Enqueue(call, CreateUncertainDecision());

        var pending = queue.GetPending(id);

        pending.Should().NotBeNull();
        pending!.QueueId.Should().Be(id);
        pending.Call.Should().Be(call);
    }

    [Fact]
    public void GetPending_InvalidId_ReturnsNull()
    {
        var queue = new ToolApprovalQueue();

        var pending = queue.GetPending("nonexistent");

        pending.Should().BeNull();
    }

    // --- Resolve ---

    [Fact]
    public async Task Resolve_Approved_ReturnsApprovedDecision()
    {
        var queue = new ToolApprovalQueue();
        var id = queue.Enqueue(CreateToolCall(), CreateUncertainDecision());

        var result = await queue.Resolve(id, approved: true, "Looks good");

        result.Certainty.Should().Be(Form.Mark);
        result.Result.IsSuccess.Should().BeTrue();
        result.Reasoning.Should().Contain("Looks good");
    }

    [Fact]
    public async Task Resolve_Rejected_ReturnsRejectedDecision()
    {
        var queue = new ToolApprovalQueue();
        var id = queue.Enqueue(CreateToolCall(), CreateUncertainDecision());

        var result = await queue.Resolve(id, approved: false, "Too risky");

        result.Certainty.Should().Be(Form.Void);
        result.Result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Resolve_RemovesFromPending()
    {
        var queue = new ToolApprovalQueue();
        var id = queue.Enqueue(CreateToolCall(), CreateUncertainDecision());

        await queue.Resolve(id, approved: true, "ok");

        queue.PendingCount.Should().Be(0);
        queue.GetPending(id).Should().BeNull();
    }

    [Fact]
    public async Task Resolve_AddsHumanReviewEvidence()
    {
        var queue = new ToolApprovalQueue();
        var id = queue.Enqueue(CreateToolCall(), CreateUncertainDecision());

        var result = await queue.Resolve(id, approved: true, "Verified safe");

        result.EvidenceTrail.Should().Contain(e => e.CriterionName == "human_review");
    }

    [Fact]
    public void Resolve_InvalidId_ThrowsInvalidOperationException()
    {
        var queue = new ToolApprovalQueue();

        var act = async () => await queue.Resolve("bad-id", true, "ok");

        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Resolve_NullQueueId_ThrowsArgumentNullException()
    {
        var queue = new ToolApprovalQueue();

        var act = async () => await queue.Resolve(null!, true, "ok");

        act.Should().ThrowAsync<ArgumentNullException>();
    }

    // --- EnqueueAndWait ---

    [Fact]
    public async Task EnqueueAndWait_ResolvesWhenHumanApproves()
    {
        var queue = new ToolApprovalQueue();
        var call = CreateToolCall();
        var decision = CreateUncertainDecision();

        var waitTask = queue.EnqueueAndWait(call, decision);

        // Find the pending item and resolve it
        var pending = await queue.GetPending();
        pending.Should().HaveCount(1);

        await queue.Resolve(pending[0].QueueId, true, "approved");

        var result = await waitTask;
        result.Certainty.Should().Be(Form.Mark);
    }

    [Fact]
    public async Task EnqueueAndWait_WithTimeout_TimesOut()
    {
        var queue = new ToolApprovalQueue();
        var call = CreateToolCall();
        var decision = CreateUncertainDecision();

        var result = await queue.EnqueueAndWait(call, decision, TimeSpan.FromMilliseconds(50));

        result.Certainty.Should().Be(Form.Imaginary);
        result.Reasoning.Should().Contain("timeout");
    }

    [Fact]
    public async Task EnqueueAndWait_WithTimeout_RemovesFromQueueOnTimeout()
    {
        var queue = new ToolApprovalQueue();
        var call = CreateToolCall();
        var decision = CreateUncertainDecision();

        await queue.EnqueueAndWait(call, decision, TimeSpan.FromMilliseconds(50));

        queue.PendingCount.Should().Be(0);
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_ValidId_ReturnsTrue()
    {
        var queue = new ToolApprovalQueue();
        var id = queue.Enqueue(CreateToolCall(), CreateUncertainDecision());

        var result = queue.Cancel(id);

        result.Should().BeTrue();
    }

    [Fact]
    public void Cancel_InvalidId_ReturnsFalse()
    {
        var queue = new ToolApprovalQueue();

        var result = queue.Cancel("nonexistent");

        result.Should().BeFalse();
    }

    [Fact]
    public void Cancel_RemovesFromPending()
    {
        var queue = new ToolApprovalQueue();
        var id = queue.Enqueue(CreateToolCall(), CreateUncertainDecision());

        queue.Cancel(id);

        queue.PendingCount.Should().Be(0);
    }

    [Fact]
    public async Task Cancel_CompletesWaitingTask()
    {
        var queue = new ToolApprovalQueue();
        var call = CreateToolCall();
        var decision = CreateUncertainDecision();

        var waitTask = queue.EnqueueAndWait(call, decision);

        var pending = await queue.GetPending();
        queue.Cancel(pending[0].QueueId);

        var result = await waitTask;
        result.Certainty.Should().Be(Form.Void);
        result.Result.Error.Should().Contain("cancelled");
    }

    // --- PendingCount ---

    [Fact]
    public void PendingCount_Empty_ReturnsZero()
    {
        var queue = new ToolApprovalQueue();

        queue.PendingCount.Should().Be(0);
    }
}
