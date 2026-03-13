using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class ToolApprovalQueueTests
{
    private static ToolCall CreateToolCall(string name = "test-tool") =>
        new(name, "{\"arg\": 1}");

    private static AuditableDecision<ToolResult> CreateUncertainDecision() =>
        AuditableDecision<ToolResult>.Uncertain("Uncertain", "Needs review");

    [Fact]
    public void Enqueue_ValidInput_ReturnsQueueId()
    {
        var sut = new ToolApprovalQueue();

        var queueId = sut.Enqueue(CreateToolCall(), CreateUncertainDecision());

        queueId.Should().NotBeNullOrEmpty();
        sut.PendingCount.Should().Be(1);
    }

    [Fact]
    public void Enqueue_NullCall_ThrowsArgumentNullException()
    {
        var sut = new ToolApprovalQueue();

        var act = () => sut.Enqueue(null!, CreateUncertainDecision());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Enqueue_NullDecision_ThrowsArgumentNullException()
    {
        var sut = new ToolApprovalQueue();

        var act = () => sut.Enqueue(CreateToolCall(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Resolve_Approved_ReturnsApprovedDecision()
    {
        var sut = new ToolApprovalQueue();
        var queueId = sut.Enqueue(CreateToolCall(), CreateUncertainDecision());

        var result = await sut.Resolve(queueId, true, "Looks good");

        result.Certainty.Should().Be(LoF.Mark);
        result.Result.IsSuccess.Should().BeTrue();
        sut.PendingCount.Should().Be(0);
    }

    [Fact]
    public async Task Resolve_Rejected_ReturnsRejectedDecision()
    {
        var sut = new ToolApprovalQueue();
        var queueId = sut.Enqueue(CreateToolCall(), CreateUncertainDecision());

        var result = await sut.Resolve(queueId, false, "Too risky");

        result.Certainty.Should().Be(LoF.Void);
        result.Result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Resolve_InvalidQueueId_ThrowsInvalidOperation()
    {
        var sut = new ToolApprovalQueue();

        var act = () => sut.Resolve("nonexistent", true, "notes");

        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void Resolve_NullQueueId_ThrowsArgumentNullException()
    {
        var sut = new ToolApprovalQueue();

        var act = () => sut.Resolve(null!, true, "notes");

        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetPending_ReturnsAllPending()
    {
        var sut = new ToolApprovalQueue();
        sut.Enqueue(CreateToolCall("tool1"), CreateUncertainDecision());
        sut.Enqueue(CreateToolCall("tool2"), CreateUncertainDecision());

        var pending = await sut.GetPending();

        pending.Should().HaveCount(2);
    }

    [Fact]
    public void GetPendingById_ExistingId_ReturnsPending()
    {
        var sut = new ToolApprovalQueue();
        var queueId = sut.Enqueue(CreateToolCall(), CreateUncertainDecision());

        var pending = sut.GetPending(queueId);

        pending.Should().NotBeNull();
        pending!.QueueId.Should().Be(queueId);
    }

    [Fact]
    public void GetPendingById_NonExistentId_ReturnsNull()
    {
        var sut = new ToolApprovalQueue();

        sut.GetPending("nonexistent").Should().BeNull();
    }

    [Fact]
    public void Cancel_ExistingId_RemovesFromQueue()
    {
        var sut = new ToolApprovalQueue();
        var queueId = sut.Enqueue(CreateToolCall(), CreateUncertainDecision());

        var cancelled = sut.Cancel(queueId);

        cancelled.Should().BeTrue();
        sut.PendingCount.Should().Be(0);
    }

    [Fact]
    public void Cancel_NonExistentId_ReturnsFalse()
    {
        var sut = new ToolApprovalQueue();

        sut.Cancel("nonexistent").Should().BeFalse();
    }

    [Fact]
    public async Task EnqueueAndWait_WithTimeout_TimesOutGracefully()
    {
        var sut = new ToolApprovalQueue();

        var result = await sut.EnqueueAndWait(
            CreateToolCall(), CreateUncertainDecision(), TimeSpan.FromMilliseconds(50));

        result.RequiresHumanReview.Should().BeTrue();
    }
}
