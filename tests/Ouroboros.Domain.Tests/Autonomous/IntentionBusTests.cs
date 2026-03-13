namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class IntentionBusTests : IDisposable
{
    private readonly IntentionBus _bus = new();

    public void Dispose() => _bus.Dispose();

    private Intention ProposeTestIntention(
        string title = "Test",
        IntentionCategory category = IntentionCategory.Learning,
        IntentionPriority priority = IntentionPriority.Normal,
        bool requiresApproval = true,
        TimeSpan? expiresIn = null)
    {
        return _bus.ProposeIntention(
            title,
            "Test description",
            "Test rationale",
            category,
            "test_source",
            priority: priority,
            requiresApproval: requiresApproval,
            expiresIn: expiresIn);
    }

    // ═══════════════════════════════════════════════════════════════
    // Construction and Properties
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_InitialState_IsNotActive()
    {
        _bus.IsActive.Should().BeFalse();
        _bus.PendingCount.Should().Be(0);
    }

    [Fact]
    public void Culture_CanBeSetAndRead()
    {
        _bus.Culture = "de-DE";
        _bus.Culture.Should().Be("de-DE");
    }

    // ═══════════════════════════════════════════════════════════════
    // Start / Stop
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Start_SetsIsActive()
    {
        _bus.Start();
        _bus.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Start_CalledTwice_DoesNotThrow()
    {
        _bus.Start();
        var act = () => _bus.Start();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task StopAsync_SetsIsInactive()
    {
        _bus.Start();
        await _bus.StopAsync();
        _bus.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task StopAsync_WhenNotActive_DoesNotThrow()
    {
        var act = () => _bus.StopAsync();
        await act.Should().NotThrowAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    // ProposeIntention
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ProposeIntention_CreatesIntentionWithPendingStatus()
    {
        var intention = ProposeTestIntention();

        intention.Should().NotBeNull();
        intention.Status.Should().Be(IntentionStatus.Pending);
        intention.Title.Should().Be("Test");
        _bus.PendingCount.Should().Be(1);
    }

    [Fact]
    public void ProposeIntention_RequiresApproval_FiresEvent()
    {
        Intention? fired = null;
        _bus.OnIntentionRequiresAttention += i => fired = i;

        ProposeTestIntention(requiresApproval: true);

        fired.Should().NotBeNull();
        fired!.Title.Should().Be("Test");
    }

    [Fact]
    public void ProposeIntention_NoApprovalRequired_DoesNotFireEvent()
    {
        Intention? fired = null;
        _bus.OnIntentionRequiresAttention += i => fired = i;

        ProposeTestIntention(requiresApproval: false);

        fired.Should().BeNull();
    }

    [Fact]
    public void ProposeIntention_WithExpiresIn_SetsExpiresAt()
    {
        var intention = ProposeTestIntention(expiresIn: TimeSpan.FromMinutes(5));

        intention.ExpiresAt.Should().NotBeNull();
        intention.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    // ═══════════════════════════════════════════════════════════════
    // Approve / Reject
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ApproveIntention_ValidPendingIntention_ReturnsTrue()
    {
        var intention = ProposeTestIntention();

        bool result = _bus.ApproveIntention(intention.Id, "Approved");

        result.Should().BeTrue();
        _bus.PendingCount.Should().Be(0);
    }

    [Fact]
    public void ApproveIntention_NonExistentId_ReturnsFalse()
    {
        bool result = _bus.ApproveIntention(Guid.NewGuid());
        result.Should().BeFalse();
    }

    [Fact]
    public void ApproveIntention_AlreadyApproved_ReturnsFalse()
    {
        var intention = ProposeTestIntention();
        _bus.ApproveIntention(intention.Id);

        bool result = _bus.ApproveIntention(intention.Id);
        result.Should().BeFalse();
    }

    [Fact]
    public void ApproveIntentionByPartialId_ValidPrefix_ReturnsTrue()
    {
        var intention = ProposeTestIntention();
        string partialId = intention.Id.ToString()[..8];

        bool result = _bus.ApproveIntentionByPartialId(partialId);
        result.Should().BeTrue();
    }

    [Fact]
    public void ApproveIntentionByPartialId_InvalidPrefix_ReturnsFalse()
    {
        ProposeTestIntention();

        bool result = _bus.ApproveIntentionByPartialId("xxxxxxxx");
        result.Should().BeFalse();
    }

    [Fact]
    public void RejectIntention_ValidPendingIntention_ReturnsTrue()
    {
        var intention = ProposeTestIntention();

        bool result = _bus.RejectIntention(intention.Id, "Not needed");

        result.Should().BeTrue();
        _bus.PendingCount.Should().Be(0);
    }

    [Fact]
    public void RejectIntention_NonExistentId_ReturnsFalse()
    {
        bool result = _bus.RejectIntention(Guid.NewGuid());
        result.Should().BeFalse();
    }

    [Fact]
    public void RejectIntentionByPartialId_ValidPrefix_ReturnsTrue()
    {
        var intention = ProposeTestIntention();
        string partialId = intention.Id.ToString()[..8];

        bool result = _bus.RejectIntentionByPartialId(partialId, "Rejected");
        result.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // Execution Lifecycle
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void MarkExecuting_ApprovedIntention_ReturnsTrue()
    {
        var intention = ProposeTestIntention();
        _bus.ApproveIntention(intention.Id);

        bool result = _bus.MarkExecuting(intention.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public void MarkExecuting_PendingIntention_ReturnsFalse()
    {
        var intention = ProposeTestIntention();

        bool result = _bus.MarkExecuting(intention.Id);
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkCompleted_ExecutingIntention_ReturnsTrue()
    {
        var intention = ProposeTestIntention();
        _bus.ApproveIntention(intention.Id);
        _bus.MarkExecuting(intention.Id);

        bool result = _bus.MarkCompleted(intention.Id, "Done");
        result.Should().BeTrue();
    }

    [Fact]
    public void MarkCompleted_PendingIntention_ReturnsFalse()
    {
        var intention = ProposeTestIntention();

        bool result = _bus.MarkCompleted(intention.Id, "Done");
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkFailed_ExecutingIntention_ReturnsTrue()
    {
        var intention = ProposeTestIntention();
        _bus.ApproveIntention(intention.Id);
        _bus.MarkExecuting(intention.Id);

        bool result = _bus.MarkFailed(intention.Id, "Error occurred");
        result.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // Queries
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void GetPendingIntentions_ReturnsOnlyPending()
    {
        var i1 = ProposeTestIntention("First");
        var i2 = ProposeTestIntention("Second");
        _bus.ApproveIntention(i1.Id);

        var pending = _bus.GetPendingIntentions();
        pending.Should().HaveCount(1);
        pending[0].Title.Should().Be("Second");
    }

    [Fact]
    public void GetPendingIntentions_OrderedByPriorityThenAge()
    {
        ProposeTestIntention("Low", priority: IntentionPriority.Low);
        ProposeTestIntention("High", priority: IntentionPriority.High);
        ProposeTestIntention("Normal", priority: IntentionPriority.Normal);

        var pending = _bus.GetPendingIntentions();
        pending[0].Title.Should().Be("High");
        pending[1].Title.Should().Be("Normal");
        pending[2].Title.Should().Be("Low");
    }

    [Fact]
    public void GetAllIntentions_ReturnsAllIntentions()
    {
        ProposeTestIntention("A");
        ProposeTestIntention("B");
        var i3 = ProposeTestIntention("C");
        _bus.ApproveIntention(i3.Id);

        _bus.GetAllIntentions().Should().HaveCount(3);
    }

    [Fact]
    public void GetIntentionsByCategory_FiltersCorrectly()
    {
        ProposeTestIntention("Learn", category: IntentionCategory.Learning);
        ProposeTestIntention("Explore", category: IntentionCategory.Exploration);

        var learning = _bus.GetIntentionsByCategory(IntentionCategory.Learning);
        learning.Should().HaveCount(1);
        learning[0].Title.Should().Be("Learn");
    }

    [Fact]
    public void GetNextApprovedIntention_ReturnsHighestPriorityFirst()
    {
        var low = ProposeTestIntention("Low", priority: IntentionPriority.Low);
        var high = ProposeTestIntention("High", priority: IntentionPriority.High);
        _bus.ApproveIntention(low.Id);
        _bus.ApproveIntention(high.Id);

        var next = _bus.GetNextApprovedIntention();
        next.Should().NotBeNull();
        next!.Title.Should().Be("High");
    }

    [Fact]
    public void GetNextApprovedIntention_NoneApproved_ReturnsNull()
    {
        ProposeTestIntention();
        _bus.GetNextApprovedIntention().Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // ApproveAllLowRisk
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ApproveAllLowRisk_ApprovesCorrectCategories()
    {
        ProposeTestIntention("Reflect", category: IntentionCategory.SelfReflection, priority: IntentionPriority.Normal);
        ProposeTestIntention("Learn", category: IntentionCategory.Learning, priority: IntentionPriority.Low);
        ProposeTestIntention("Code", category: IntentionCategory.CodeModification, priority: IntentionPriority.Low);

        int approved = _bus.ApproveAllLowRisk();

        approved.Should().Be(2);
        _bus.PendingCount.Should().Be(1);
    }

    [Fact]
    public void ApproveAllLowRisk_HighPriorityLearning_NotApproved()
    {
        ProposeTestIntention("HighLearn", category: IntentionCategory.Learning, priority: IntentionPriority.High);

        int approved = _bus.ApproveAllLowRisk();
        approved.Should().Be(0);
    }

    // ═══════════════════════════════════════════════════════════════
    // Localization
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Start_GermanCulture_SendsLocalizedMessage()
    {
        _bus.Culture = "de-DE";
        string? receivedMessage = null;
        _bus.OnProactiveMessage += (msg, _) => receivedMessage = msg;

        _bus.Start();

        receivedMessage.Should().Contain("aktiviert");
    }

    [Fact]
    public void Start_EnglishCulture_SendsEnglishMessage()
    {
        string? receivedMessage = null;
        _bus.OnProactiveMessage += (msg, _) => receivedMessage = msg;

        _bus.Start();

        receivedMessage.Should().Contain("activated");
    }

    // ═══════════════════════════════════════════════════════════════
    // GetSummary
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void GetSummary_ContainsStatusInformation()
    {
        ProposeTestIntention("A");
        ProposeTestIntention("B");

        string summary = _bus.GetSummary();

        summary.Should().Contain("IntentionBus Status");
        summary.Should().Contain("Pending: 2");
    }
}
