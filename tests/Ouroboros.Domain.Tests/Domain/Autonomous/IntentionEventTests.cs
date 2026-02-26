namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class IntentionEventTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var intention = new Intention
        {
            Title = "Test",
            Description = "Desc",
            Rationale = "Rationale",
            Category = IntentionCategory.Learning,
            Source = "Src",
        };
        var timestamp = DateTime.UtcNow;

        // Act
        var evt = new IntentionEvent(intention, IntentionStatus.Pending, IntentionStatus.Approved, timestamp);

        // Assert
        evt.Intention.Should().Be(intention);
        evt.OldStatus.Should().Be(IntentionStatus.Pending);
        evt.NewStatus.Should().Be(IntentionStatus.Approved);
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var intention = new Intention
        {
            Title = "Test",
            Description = "Desc",
            Rationale = "Rationale",
            Category = IntentionCategory.Learning,
            Source = "Src",
        };
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var evt1 = new IntentionEvent(intention, IntentionStatus.Pending, IntentionStatus.Approved, timestamp);
        var evt2 = new IntentionEvent(intention, IntentionStatus.Pending, IntentionStatus.Approved, timestamp);

        // Assert
        evt1.Should().Be(evt2);
    }
}
