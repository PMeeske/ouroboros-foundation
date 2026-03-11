using Ouroboros.Agent.TemporalReasoning;

namespace Ouroboros.Abstractions.Tests.Agent.TemporalReasoning;

[Trait("Category", "Unit")]
public class TemporalReasoningAdditionalTests
{
    private static TemporalEvent CreateEvent(
        string type = "test",
        DateTime? start = null) =>
        new TemporalEvent(
            Guid.NewGuid(), type, "desc",
            start ?? DateTime.UtcNow,
            null,
            new Dictionary<string, object>(),
            new List<string>());

    [Fact]
    public void TemporalQuery_RecordEquality_SameDefaults_AreEqual()
    {
        // Arrange
        var a = new TemporalQuery();
        var b = new TemporalQuery();

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TemporalQuery_AllParametersSet()
    {
        // Arrange
        var relatedId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var query = new TemporalQuery(
            StartAfter: now.AddDays(-7),
            StartBefore: now,
            EndAfter: now.AddDays(-7),
            EndBefore: now,
            EventType: "login",
            MaxResults: 50,
            After: now.AddDays(-1),
            Before: now,
            Duration: TimeSpan.FromHours(1),
            RelatedEventId: relatedId);

        // Assert
        query.StartAfter.Should().NotBeNull();
        query.StartBefore.Should().NotBeNull();
        query.EndAfter.Should().NotBeNull();
        query.EndBefore.Should().NotBeNull();
        query.Duration.Should().Be(TimeSpan.FromHours(1));
        query.RelatedEventId.Should().Be(relatedId);
    }

    [Fact]
    public void TemporalConstraint_WithMaxGapOnly_MinGapIsNull()
    {
        // Act
        var constraint = new TemporalConstraint(
            "a", "b", TemporalRelation.Before,
            MaxGap: TimeSpan.FromHours(2));

        // Assert
        constraint.MinGap.Should().BeNull();
        constraint.MaxGap.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void Timeline_EmptyTimeline_IsValid()
    {
        // Arrange
        var ts = DateTime.UtcNow;

        // Act
        var timeline = new Timeline(
            new List<TemporalEvent>(),
            new List<TemporalRelationEdge>(),
            ts, ts,
            new Dictionary<string, IReadOnlyList<TemporalEvent>>());

        // Assert
        timeline.Events.Should().BeEmpty();
        timeline.Relations.Should().BeEmpty();
        timeline.EventsByType.Should().BeEmpty();
    }

    [Fact]
    public void PredictedEvent_LowConfidence_IsValid()
    {
        // Act
        var predicted = new PredictedEvent(
            "type", "desc", DateTime.UtcNow.AddDays(1),
            0.0, new List<TemporalEvent>(), "Pure guess");

        // Assert
        predicted.Confidence.Should().Be(0.0);
    }

    [Fact]
    public void CausalRelation_EmptyConfounders_IsValid()
    {
        // Arrange
        var cause = CreateEvent("cause");
        var effect = CreateEvent("effect");

        // Act
        var relation = new CausalRelation(
            cause, effect, 1.0, "direct", new List<string>());

        // Assert
        relation.ConfoundingFactors.Should().BeEmpty();
        relation.CausalStrength.Should().Be(1.0);
    }

    [Fact]
    public void TemporalRelationEdge_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var event1 = CreateEvent("a");
        var event2 = CreateEvent("b");
        var original = new TemporalRelationEdge(
            event1, event2, TemporalRelationType.Before, 0.5);

        // Act
        var modified = original with { Confidence = 0.99 };

        // Assert
        modified.Confidence.Should().Be(0.99);
        modified.RelationType.Should().Be(TemporalRelationType.Before);
    }
}
