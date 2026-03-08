namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class IntentionBusTests : IDisposable
{
    private readonly IntentionBus _sut = new();

    [Fact]
    public void IsActive_InitiallyFalse()
    {
        _sut.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Start_SetsIsActiveTrue()
    {
        // Act
        _sut.Start();

        // Assert
        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Start_CalledTwice_DoesNotThrow()
    {
        // Act
        _sut.Start();
        _sut.Start();

        // Assert
        _sut.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_SetsIsActiveFalse()
    {
        // Arrange
        _sut.Start();

        // Act
        await _sut.StopAsync();

        // Assert
        _sut.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task StopAsync_WhenNotStarted_DoesNotThrow()
    {
        // Act & Assert
        await _sut.StopAsync();
        _sut.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ProposeIntention_CreatesIntention()
    {
        // Act
        Intention intention = _sut.ProposeIntention(
            "Test Title",
            "Test Description",
            "Test Rationale",
            IntentionCategory.Learning,
            "TestNeuron");

        // Assert
        intention.Should().NotBeNull();
        intention.Title.Should().Be("Test Title");
        intention.Description.Should().Be("Test Description");
        intention.Status.Should().Be(IntentionStatus.Pending);
    }

    [Fact]
    public void ProposeIntention_AddsToPendingList()
    {
        // Act
        _sut.ProposeIntention("Title", "Desc", "Rationale", IntentionCategory.Learning, "Source");

        // Assert
        _sut.PendingCount.Should().Be(1);
        _sut.GetPendingIntentions().Should().HaveCount(1);
    }

    [Fact]
    public void ProposeIntention_WithExpiration_SetsExpiresAt()
    {
        // Act
        var intention = _sut.ProposeIntention(
            "Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source",
            expiresIn: TimeSpan.FromMinutes(5));

        // Assert
        intention.ExpiresAt.Should().NotBeNull();
        intention.ExpiresAt!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ProposeIntention_NoApprovalRequired_DoesNotFireAttentionEvent()
    {
        // Arrange
        bool attentionFired = false;
        _sut.OnIntentionRequiresAttention += _ => attentionFired = true;

        // Act
        _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source",
            requiresApproval: false);

        // Assert
        attentionFired.Should().BeFalse();
    }

    [Fact]
    public void ProposeIntention_RequiresApproval_FiresAttentionEvent()
    {
        // Arrange
        bool attentionFired = false;
        _sut.OnIntentionRequiresAttention += _ => attentionFired = true;

        // Act
        _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source",
            requiresApproval: true);

        // Assert
        attentionFired.Should().BeTrue();
    }

    [Fact]
    public void ApproveIntention_ValidId_ReturnsTrue()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");

        // Act
        bool result = _sut.ApproveIntention(intention.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ApproveIntention_InvalidId_ReturnsFalse()
    {
        // Act
        bool result = _sut.ApproveIntention(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApproveIntention_AlreadyApproved_ReturnsFalse()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");
        _sut.ApproveIntention(intention.Id);

        // Act
        bool result = _sut.ApproveIntention(intention.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RejectIntention_ValidId_ReturnsTrue()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");

        // Act
        bool result = _sut.RejectIntention(intention.Id, "Not needed");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RejectIntention_InvalidId_ReturnsFalse()
    {
        // Act
        bool result = _sut.RejectIntention(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RejectIntention_AlreadyRejected_ReturnsFalse()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");
        _sut.RejectIntention(intention.Id);

        // Act
        bool result = _sut.RejectIntention(intention.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApproveIntentionByPartialId_MatchesPartialId()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");
        string partialId = intention.Id.ToString()[..8];

        // Act
        bool result = _sut.ApproveIntentionByPartialId(partialId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ApproveIntentionByPartialId_NoMatch_ReturnsFalse()
    {
        // Act
        bool result = _sut.ApproveIntentionByPartialId("00000000");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RejectIntentionByPartialId_MatchesPartialId()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");
        string partialId = intention.Id.ToString()[..8];

        // Act
        bool result = _sut.RejectIntentionByPartialId(partialId, "No reason");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetNextApprovedIntention_ReturnsHighestPriority()
    {
        // Arrange
        var lowPriority = _sut.ProposeIntention("Low", "Desc", "Rationale",
            IntentionCategory.Learning, "Source", priority: IntentionPriority.Low);
        var highPriority = _sut.ProposeIntention("High", "Desc", "Rationale",
            IntentionCategory.Learning, "Source", priority: IntentionPriority.High);

        _sut.ApproveIntention(lowPriority.Id);
        _sut.ApproveIntention(highPriority.Id);

        // Act
        var next = _sut.GetNextApprovedIntention();

        // Assert
        next.Should().NotBeNull();
        next!.Title.Should().Be("High");
    }

    [Fact]
    public void GetNextApprovedIntention_NoneApproved_ReturnsNull()
    {
        // Arrange
        _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");

        // Act
        var next = _sut.GetNextApprovedIntention();

        // Assert
        next.Should().BeNull();
    }

    [Fact]
    public void MarkExecuting_ApprovedIntention_ReturnsTrue()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");
        _sut.ApproveIntention(intention.Id);

        // Act
        bool result = _sut.MarkExecuting(intention.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MarkExecuting_PendingIntention_ReturnsFalse()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");

        // Act
        bool result = _sut.MarkExecuting(intention.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkCompleted_ExecutingIntention_ReturnsTrue()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");
        _sut.ApproveIntention(intention.Id);
        _sut.MarkExecuting(intention.Id);

        // Act
        bool result = _sut.MarkCompleted(intention.Id, "Done");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MarkCompleted_NotExecuting_ReturnsFalse()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");
        _sut.ApproveIntention(intention.Id);

        // Act
        bool result = _sut.MarkCompleted(intention.Id, "Done");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void MarkFailed_ValidIntention_ReturnsTrue()
    {
        // Arrange
        var intention = _sut.ProposeIntention("Test", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");

        // Act
        bool result = _sut.MarkFailed(intention.Id, "error");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MarkFailed_InvalidId_ReturnsFalse()
    {
        // Act
        bool result = _sut.MarkFailed(Guid.NewGuid(), "error");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ApproveAllLowRisk_ApprovesLearningAndSelfReflectionAndMemory()
    {
        // Arrange
        _sut.ProposeIntention("Learn", "Desc", "Rationale",
            IntentionCategory.Learning, "Source");
        _sut.ProposeIntention("Reflect", "Desc", "Rationale",
            IntentionCategory.SelfReflection, "Source");
        _sut.ProposeIntention("Memory", "Desc", "Rationale",
            IntentionCategory.MemoryManagement, "Source");
        _sut.ProposeIntention("Code", "Desc", "Rationale",
            IntentionCategory.CodeModification, "Source");

        // Act
        int approved = _sut.ApproveAllLowRisk();

        // Assert
        approved.Should().Be(3);
        _sut.PendingCount.Should().Be(1);
    }

    [Fact]
    public void ApproveAllLowRisk_DoesNotApproveHighPriority()
    {
        // Arrange
        _sut.ProposeIntention("Learn", "Desc", "Rationale",
            IntentionCategory.Learning, "Source",
            priority: IntentionPriority.High);

        // Act
        int approved = _sut.ApproveAllLowRisk();

        // Assert
        approved.Should().Be(0);
    }

    [Fact]
    public void GetAllIntentions_ReturnsAll()
    {
        // Arrange
        _sut.ProposeIntention("A", "Desc", "Rationale", IntentionCategory.Learning, "Source");
        _sut.ProposeIntention("B", "Desc", "Rationale", IntentionCategory.Learning, "Source");

        // Act
        var all = _sut.GetAllIntentions();

        // Assert
        all.Should().HaveCount(2);
    }

    [Fact]
    public void GetIntentionsByCategory_FiltersCorrectly()
    {
        // Arrange
        _sut.ProposeIntention("Learn", "Desc", "Rationale", IntentionCategory.Learning, "Source");
        _sut.ProposeIntention("Code", "Desc", "Rationale", IntentionCategory.CodeModification, "Source");

        // Act
        var learning = _sut.GetIntentionsByCategory(IntentionCategory.Learning);

        // Assert
        learning.Should().HaveCount(1);
        learning[0].Title.Should().Be("Learn");
    }

    [Fact]
    public void GetSummary_ReturnsFormattedString()
    {
        // Arrange
        _sut.ProposeIntention("Test", "Desc", "Rationale", IntentionCategory.Learning, "Source");

        // Act
        string summary = _sut.GetSummary();

        // Assert
        summary.Should().Contain("Pending: 1");
        summary.Should().Contain("IntentionBus Status");
    }

    [Fact]
    public void GetPendingIntentions_OrdersByPriorityThenAge()
    {
        // Arrange
        _sut.ProposeIntention("Low", "Desc", "Rationale",
            IntentionCategory.Learning, "Source",
            priority: IntentionPriority.Low);
        _sut.ProposeIntention("High", "Desc", "Rationale",
            IntentionCategory.Learning, "Source",
            priority: IntentionPriority.High);
        _sut.ProposeIntention("Normal", "Desc", "Rationale",
            IntentionCategory.Learning, "Source",
            priority: IntentionPriority.Normal);

        // Act
        var pending = _sut.GetPendingIntentions();

        // Assert
        pending.Should().HaveCount(3);
        pending[0].Title.Should().Be("High");
    }

    [Fact]
    public void Localize_German_ProducesGermanMessage()
    {
        // Arrange
        _sut.Culture = "de-DE";
        string? capturedMessage = null;
        _sut.OnProactiveMessage += (msg, _) => capturedMessage = msg;

        // Act
        _sut.Start();

        // Assert
        capturedMessage.Should().Contain("aktiviert");
    }

    [Fact]
    public void Localize_Default_ProducesEnglishMessage()
    {
        // Arrange
        string? capturedMessage = null;
        _sut.OnProactiveMessage += (msg, _) => capturedMessage = msg;

        // Act
        _sut.Start();

        // Assert
        capturedMessage.Should().Contain("activated");
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
