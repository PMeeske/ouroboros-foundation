using Ouroboros.Domain.Autonomous;

namespace Ouroboros.Tests.Autonomous;

[Trait("Category", "Unit")]
public class IntentionBusTests : IDisposable
{
    private readonly IntentionBus _bus = new();

    public void Dispose() => _bus.Dispose();

    private Intention ProposeDefault(
        string title = "Test",
        IntentionCategory category = IntentionCategory.Learning,
        IntentionPriority priority = IntentionPriority.Normal,
        bool requiresApproval = true)
    {
        return _bus.ProposeIntention(
            title: title,
            description: "Test description",
            rationale: "Test rationale",
            category: category,
            source: "TestSource",
            priority: priority,
            requiresApproval: requiresApproval);
    }

    // --- ProposeIntention ---

    [Fact]
    public void ProposeIntention_AddsToStore()
    {
        // Act
        var intention = ProposeDefault();

        // Assert
        _bus.PendingCount.Should().Be(1);
        _bus.GetAllIntentions().Should().ContainSingle();
        intention.Status.Should().Be(IntentionStatus.Pending);
    }

    [Fact]
    public void ProposeIntention_FiresAttentionEvent()
    {
        // Arrange
        Intention? captured = null;
        _bus.OnIntentionRequiresAttention += i => captured = i;

        // Act
        _ = ProposeDefault();

        // Assert
        captured.Should().NotBeNull();
        captured!.Title.Should().Be("Test");
    }

    [Fact]
    public void ProposeIntention_WithoutApproval_DoesNotFireAttentionEvent()
    {
        // Arrange
        bool fired = false;
        _bus.OnIntentionRequiresAttention += _ => fired = true;

        // Act
        ProposeDefault(requiresApproval: false);

        // Assert
        fired.Should().BeFalse();
    }

    // --- ApproveIntention ---

    [Fact]
    public void ApproveIntention_PendingIntention_Succeeds()
    {
        // Arrange
        var intention = ProposeDefault();

        // Act
        bool result = _bus.ApproveIntention(intention.Id, "Looks good");

        // Assert
        result.Should().BeTrue();
        _bus.PendingCount.Should().Be(0);
    }

    [Fact]
    public void ApproveIntention_NonexistentId_ReturnsFalse()
    {
        // Act
        bool result = _bus.ApproveIntention(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApproveIntention_AlreadyApproved_ReturnsFalse()
    {
        // Arrange
        var intention = ProposeDefault();
        _bus.ApproveIntention(intention.Id);

        // Act
        bool result = _bus.ApproveIntention(intention.Id);

        // Assert
        result.Should().BeFalse();
    }

    // --- RejectIntention ---

    [Fact]
    public void RejectIntention_PendingIntention_Succeeds()
    {
        // Arrange
        var intention = ProposeDefault();

        // Act
        bool result = _bus.RejectIntention(intention.Id, "Not now");

        // Assert
        result.Should().BeTrue();
        _bus.PendingCount.Should().Be(0);
    }

    [Fact]
    public void RejectIntention_NonexistentId_ReturnsFalse()
    {
        // Act
        bool result = _bus.RejectIntention(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    // --- Partial ID matching ---

    [Fact]
    public void ApproveIntentionByPartialId_MatchesPrefix()
    {
        // Arrange
        var intention = ProposeDefault();
        string partialId = intention.Id.ToString()[..8];

        // Act
        bool result = _bus.ApproveIntentionByPartialId(partialId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RejectIntentionByPartialId_MatchesPrefix()
    {
        // Arrange
        var intention = ProposeDefault();
        string partialId = intention.Id.ToString()[..8];

        // Act
        bool result = _bus.RejectIntentionByPartialId(partialId, "Rejected");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ApproveIntentionByPartialId_NoMatch_ReturnsFalse()
    {
        // Act
        bool result = _bus.ApproveIntentionByPartialId("00000000");

        // Assert
        result.Should().BeFalse();
    }

    // --- GetNextApprovedIntention ---

    [Fact]
    public void GetNextApprovedIntention_ReturnsHighestPriority()
    {
        // Arrange
        var low = ProposeDefault("Low", priority: IntentionPriority.Low);
        var high = ProposeDefault("High", priority: IntentionPriority.High);
        _bus.ApproveIntention(low.Id);
        _bus.ApproveIntention(high.Id);

        // Act
        var next = _bus.GetNextApprovedIntention();

        // Assert
        next.Should().NotBeNull();
        next!.Title.Should().Be("High");
    }

    [Fact]
    public void GetNextApprovedIntention_NoApproved_ReturnsNull()
    {
        // Arrange
        ProposeDefault();

        // Act
        var next = _bus.GetNextApprovedIntention();

        // Assert
        next.Should().BeNull();
    }

    // --- MarkExecuting ---

    [Fact]
    public void MarkExecuting_ApprovedIntention_Succeeds()
    {
        // Arrange
        var intention = ProposeDefault();
        _bus.ApproveIntention(intention.Id);

        // Act
        bool result = _bus.MarkExecuting(intention.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MarkExecuting_PendingIntention_Fails()
    {
        // Arrange
        var intention = ProposeDefault();

        // Act
        bool result = _bus.MarkExecuting(intention.Id);

        // Assert
        result.Should().BeFalse();
    }

    // --- MarkCompleted ---

    [Fact]
    public void MarkCompleted_ExecutingIntention_Succeeds()
    {
        // Arrange
        var intention = ProposeDefault();
        _bus.ApproveIntention(intention.Id);
        _bus.MarkExecuting(intention.Id);

        // Act
        bool result = _bus.MarkCompleted(intention.Id, "Done!");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MarkCompleted_PendingIntention_Fails()
    {
        // Arrange
        var intention = ProposeDefault();

        // Act
        bool result = _bus.MarkCompleted(intention.Id, "Done");

        // Assert
        result.Should().BeFalse();
    }

    // --- MarkFailed ---

    [Fact]
    public void MarkFailed_RecordsFailure()
    {
        // Arrange
        var intention = ProposeDefault();
        _bus.ApproveIntention(intention.Id);
        _bus.MarkExecuting(intention.Id);

        // Act
        bool result = _bus.MarkFailed(intention.Id, "Connection timeout");

        // Assert
        result.Should().BeTrue();
    }

    // --- ApproveAllLowRisk ---

    [Fact]
    public void ApproveAllLowRisk_ApprovesLowRiskCategories()
    {
        // Arrange
        ProposeDefault("Learning", IntentionCategory.Learning, IntentionPriority.Normal);
        ProposeDefault("Memory", IntentionCategory.MemoryManagement, IntentionPriority.Low);
        ProposeDefault("Reflection", IntentionCategory.SelfReflection, IntentionPriority.Normal);
        ProposeDefault("CodeMod", IntentionCategory.CodeModification, IntentionPriority.Normal); // Should NOT be auto-approved

        // Act
        int approved = _bus.ApproveAllLowRisk();

        // Assert
        approved.Should().Be(3);
        _bus.PendingCount.Should().Be(1); // CodeModification remains pending
    }

    [Fact]
    public void ApproveAllLowRisk_DoesNotApproveHighPriority()
    {
        // Arrange
        ProposeDefault("High", IntentionCategory.Learning, IntentionPriority.High);

        // Act
        int approved = _bus.ApproveAllLowRisk();

        // Assert
        approved.Should().Be(0);
    }

    // --- GetPendingIntentions ---

    [Fact]
    public void GetPendingIntentions_OrdersByPriorityThenAge()
    {
        // Arrange
        ProposeDefault("Low", priority: IntentionPriority.Low);
        ProposeDefault("High", priority: IntentionPriority.High);
        ProposeDefault("Normal", priority: IntentionPriority.Normal);

        // Act
        var pending = _bus.GetPendingIntentions();

        // Assert
        pending.Should().HaveCount(3);
        pending[0].Title.Should().Be("High");
    }

    // --- GetIntentionsByCategory ---

    [Fact]
    public void GetIntentionsByCategory_FiltersCorrectly()
    {
        // Arrange
        ProposeDefault("Learn1", IntentionCategory.Learning);
        ProposeDefault("Code1", IntentionCategory.CodeModification);
        ProposeDefault("Learn2", IntentionCategory.Learning);

        // Act
        var learning = _bus.GetIntentionsByCategory(IntentionCategory.Learning);

        // Assert
        learning.Should().HaveCount(2);
    }

    // --- GetSummary ---

    [Fact]
    public void GetSummary_ReturnsFormattedString()
    {
        // Arrange
        ProposeDefault("A");
        ProposeDefault("B");

        // Act
        string summary = _bus.GetSummary();

        // Assert
        summary.Should().Contain("Pending: 2");
        summary.Should().Contain("Total: 2");
    }

    // --- Start / IsActive ---

    [Fact]
    public void Start_ActivatesBus()
    {
        // Act
        _bus.Start();

        // Assert
        _bus.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Start_CalledTwice_IsIdempotent()
    {
        // Act
        _bus.Start();
        _bus.Start();

        // Assert
        _bus.IsActive.Should().BeTrue();
    }

    // --- StopAsync ---

    [Fact]
    public async Task StopAsync_DeactivatesBus()
    {
        // Arrange
        _bus.Start();

        // Act
        await _bus.StopAsync();

        // Assert
        _bus.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task StopAsync_WhenNotStarted_IsNoOp()
    {
        // Act
        await _bus.StopAsync();

        // Assert
        _bus.IsActive.Should().BeFalse();
    }

    // --- Localization ---

    [Fact]
    public void ProactiveMessage_WithGermanCulture_Localizes()
    {
        // Arrange
        string? captured = null;
        _bus.OnProactiveMessage += (msg, _) => captured = msg;
        _bus.Culture = "de-DE";

        // Act
        _bus.Start();

        // Assert
        captured.Should().NotBeNull();
        captured.Should().Contain("aktiviert");
    }

    // --- Expiration handling ---

    [Fact]
    public void ProposeIntention_WithExpiry_SetsExpiresAt()
    {
        // Act
        var intention = _bus.ProposeIntention(
            title: "Expiring",
            description: "D",
            rationale: "R",
            category: IntentionCategory.Learning,
            source: "S",
            expiresIn: TimeSpan.FromMinutes(5));

        // Assert
        intention.ExpiresAt.Should().NotBeNull();
        intention.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
