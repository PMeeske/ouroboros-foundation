using Ouroboros.Agent.TemporalReasoning;

namespace Ouroboros.Abstractions.Tests.Agent.TemporalReasoning;

/// <summary>
/// Additional tests for temporal reasoning types covering record equality,
/// with-expressions, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class TemporalReasoningEdgeCaseTests
{
    private static TemporalEvent CreateEvent(
        string type = "test",
        string desc = "A test event",
        DateTime? start = null) =>
        new TemporalEvent(
            Guid.NewGuid(), type, desc,
            start ?? DateTime.UtcNow,
            null,
            new Dictionary<string, object>(),
            new List<string>());

    [Fact]
    public void TemporalEvent_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var props = new Dictionary<string, object>();
        var participants = new List<string>();

        var a = new TemporalEvent(id, "type", "desc", ts, null, props, participants);
        var b = new TemporalEvent(id, "type", "desc", ts, null, props, participants);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TemporalEvent_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = CreateEvent("login", "User logged in");

        // Act
        var modified = original with { EventType = "logout", Description = "User logged out" };

        // Assert
        modified.EventType.Should().Be("logout");
        modified.Description.Should().Be("User logged out");
        modified.Id.Should().Be(original.Id);
        modified.StartTime.Should().Be(original.StartTime);
    }

    [Fact]
    public void TemporalConstraint_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new TemporalConstraint("a", "b", TemporalRelation.Before);
        var b = new TemporalConstraint("a", "b", TemporalRelation.Before);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TemporalConstraint_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new TemporalConstraint(
            "a", "b", TemporalRelation.Before,
            TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));

        // Act
        var modified = original with { RequiredRelation = TemporalRelation.After };

        // Assert
        modified.RequiredRelation.Should().Be(TemporalRelation.After);
        modified.EventIdA.Should().Be("a");
        modified.MinGap.Should().Be(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CausalRelation_RecordEquality_SameReferences_AreEqual()
    {
        // Arrange
        var cause = CreateEvent("cause");
        var effect = CreateEvent("effect");
        var confounders = new List<string>();

        var a = new CausalRelation(cause, effect, 0.9, "mechanism", confounders);
        var b = new CausalRelation(cause, effect, 0.9, "mechanism", confounders);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void CausalRelation_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var cause = CreateEvent("cause");
        var effect = CreateEvent("effect");
        var original = new CausalRelation(cause, effect, 0.5, "unknown", new List<string>());

        // Act
        var modified = original with { CausalStrength = 0.95, Mechanism = "direct" };

        // Assert
        modified.CausalStrength.Should().Be(0.95);
        modified.Mechanism.Should().Be("direct");
        modified.Cause.Should().Be(cause);
    }

    [Fact]
    public void PredictedEvent_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var basedOn = new List<TemporalEvent>();
        var time = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new PredictedEvent("type", "desc", time, 0.8, basedOn, "reason");
        var b = new PredictedEvent("type", "desc", time, 0.8, basedOn, "reason");

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void PredictedEvent_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new PredictedEvent(
            "failure", "Predicted failure",
            DateTime.UtcNow.AddHours(1), 0.6,
            new List<TemporalEvent>(), "Pattern match");

        // Act
        var modified = original with { Confidence = 0.95 };

        // Assert
        modified.Confidence.Should().Be(0.95);
        modified.EventType.Should().Be("failure");
    }

    [Fact]
    public void TemporalQuery_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new TemporalQuery(EventType: "login", MaxResults: 50);

        // Act
        var modified = original with { MaxResults = 200, EventType = "logout" };

        // Assert
        modified.MaxResults.Should().Be(200);
        modified.EventType.Should().Be("logout");
    }

    [Fact]
    public void Timeline_RecordEquality_SameReferences_AreEqual()
    {
        // Arrange
        var events = new List<TemporalEvent>();
        var relations = new List<TemporalRelationEdge>();
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var eventsByType = new Dictionary<string, IReadOnlyList<TemporalEvent>>();

        var a = new Timeline(events, relations, ts, ts, eventsByType);
        var b = new Timeline(events, relations, ts, ts, eventsByType);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TemporalRelationEdge_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var evt1 = CreateEvent("a");
        var evt2 = CreateEvent("b");

        var a = new TemporalRelationEdge(evt1, evt2, TemporalRelationType.Before, 0.9);
        var b = new TemporalRelationEdge(evt1, evt2, TemporalRelationType.Before, 0.9);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TemporalRelation_AllAllenRelations_AreDefined()
    {
        // Assert - Allen's interval algebra relations
        var values = Enum.GetValues<TemporalRelation>();
        values.Should().Contain(TemporalRelation.Before);
        values.Should().Contain(TemporalRelation.After);
        values.Should().Contain(TemporalRelation.Overlaps);
        values.Should().Contain(TemporalRelation.Simultaneous);
        values.Should().Contain(TemporalRelation.Contains);
        values.Should().Contain(TemporalRelation.During);
        values.Should().Contain(TemporalRelation.Meets);
        values.Should().Contain(TemporalRelation.Unknown);
    }

    [Fact]
    public void TemporalEvent_WithEndTime_BothTimesAccessible()
    {
        // Arrange
        var start = new DateTime(2025, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2025, 1, 1, 17, 0, 0, DateTimeKind.Utc);

        // Act
        var evt = new TemporalEvent(
            Guid.NewGuid(), "work", "Work day",
            start, end,
            new Dictionary<string, object>(),
            new List<string> { "employee-1" });

        // Assert
        evt.StartTime.Should().Be(start);
        evt.EndTime.Should().Be(end);
        (evt.EndTime!.Value - evt.StartTime).TotalHours.Should().Be(8);
    }
}
