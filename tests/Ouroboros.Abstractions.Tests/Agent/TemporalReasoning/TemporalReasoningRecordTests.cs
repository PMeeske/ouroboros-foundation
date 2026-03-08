using Ouroboros.Agent.TemporalReasoning;

namespace Ouroboros.Abstractions.Tests.Agent.TemporalReasoning;

[Trait("Category", "Unit")]
public class TemporalReasoningRecordTests
{
    private static TemporalEvent CreateEvent(
        string type = "test",
        string desc = "A test event",
        DateTime? start = null,
        DateTime? end = null)
    {
        return new TemporalEvent(
            Guid.NewGuid(), type, desc,
            start ?? DateTime.UtcNow,
            end,
            new Dictionary<string, object>(),
            new List<string>());
    }

    [Fact]
    public void TemporalEvent_AllPropertiesSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var start = DateTime.UtcNow;
        var end = start.AddHours(1);
        var props = new Dictionary<string, object> { ["location"] = "office" };
        var participants = new List<string> { "agent-1" };

        // Act
        var evt = new TemporalEvent(id, "meeting", "Team meeting", start, end, props, participants);

        // Assert
        evt.Id.Should().Be(id);
        evt.EventType.Should().Be("meeting");
        evt.Description.Should().Be("Team meeting");
        evt.StartTime.Should().Be(start);
        evt.EndTime.Should().Be(end);
        evt.Properties.Should().ContainKey("location");
        evt.Participants.Should().Contain("agent-1");
    }

    [Fact]
    public void TemporalEvent_NullEndTime_IsAllowed()
    {
        // Act
        var evt = CreateEvent(end: null);

        // Assert
        evt.EndTime.Should().BeNull();
    }

    [Fact]
    public void TemporalRelation_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<TemporalRelation>();

        // Assert
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
    public void TemporalRelationType_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<TemporalRelationType>();

        // Assert
        values.Should().HaveCountGreaterThanOrEqualTo(13);
        values.Should().Contain(TemporalRelationType.Before);
        values.Should().Contain(TemporalRelationType.After);
        values.Should().Contain(TemporalRelationType.Meets);
        values.Should().Contain(TemporalRelationType.MetBy);
        values.Should().Contain(TemporalRelationType.Overlaps);
        values.Should().Contain(TemporalRelationType.OverlappedBy);
        values.Should().Contain(TemporalRelationType.During);
        values.Should().Contain(TemporalRelationType.Contains);
        values.Should().Contain(TemporalRelationType.Starts);
        values.Should().Contain(TemporalRelationType.StartedBy);
        values.Should().Contain(TemporalRelationType.Finishes);
        values.Should().Contain(TemporalRelationType.FinishedBy);
        values.Should().Contain(TemporalRelationType.Equals);
    }

    [Fact]
    public void TemporalRelationEdge_AllPropertiesSet()
    {
        // Arrange
        var event1 = CreateEvent("login", "User logged in");
        var event2 = CreateEvent("action", "User performed action");

        // Act
        var edge = new TemporalRelationEdge(
            event1, event2, TemporalRelationType.Before, 0.95);

        // Assert
        edge.Event1.Should().Be(event1);
        edge.Event2.Should().Be(event2);
        edge.RelationType.Should().Be(TemporalRelationType.Before);
        edge.Confidence.Should().Be(0.95);
    }

    [Fact]
    public void CausalRelation_AllPropertiesSet()
    {
        // Arrange
        var cause = CreateEvent("spike", "CPU spike");
        var effect = CreateEvent("slowdown", "Response slowdown");
        var confounders = new List<string> { "concurrent users", "disk IO" };

        // Act
        var relation = new CausalRelation(
            cause, effect, 0.85, "resource contention", confounders);

        // Assert
        relation.Cause.Should().Be(cause);
        relation.Effect.Should().Be(effect);
        relation.CausalStrength.Should().Be(0.85);
        relation.Mechanism.Should().Be("resource contention");
        relation.ConfoundingFactors.Should().HaveCount(2);
    }

    [Fact]
    public void PredictedEvent_AllPropertiesSet()
    {
        // Arrange
        var basedOn = new List<TemporalEvent> { CreateEvent() };
        var predictedTime = DateTime.UtcNow.AddHours(1);

        // Act
        var predicted = new PredictedEvent(
            "failure", "Server failure predicted", predictedTime,
            0.75, basedOn, "Pattern matches previous incidents");

        // Assert
        predicted.EventType.Should().Be("failure");
        predicted.Description.Should().Be("Server failure predicted");
        predicted.PredictedTime.Should().Be(predictedTime);
        predicted.Confidence.Should().Be(0.75);
        predicted.BasedOnEvents.Should().HaveCount(1);
        predicted.ReasoningExplanation.Should().Contain("Pattern");
    }

    [Fact]
    public void TemporalConstraint_AllPropertiesSet()
    {
        // Act
        var constraint = new TemporalConstraint(
            "evt-1", "evt-2", TemporalRelation.Before,
            TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));

        // Assert
        constraint.EventIdA.Should().Be("evt-1");
        constraint.EventIdB.Should().Be("evt-2");
        constraint.RequiredRelation.Should().Be(TemporalRelation.Before);
        constraint.MinGap.Should().Be(TimeSpan.FromMinutes(5));
        constraint.MaxGap.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void TemporalConstraint_DefaultGaps_AreNull()
    {
        // Act
        var constraint = new TemporalConstraint(
            "a", "b", TemporalRelation.Simultaneous);

        // Assert
        constraint.MinGap.Should().BeNull();
        constraint.MaxGap.Should().BeNull();
    }

    [Fact]
    public void TemporalQuery_DefaultValues_AreCorrect()
    {
        // Act
        var query = new TemporalQuery();

        // Assert
        query.StartAfter.Should().BeNull();
        query.StartBefore.Should().BeNull();
        query.EndAfter.Should().BeNull();
        query.EndBefore.Should().BeNull();
        query.EventType.Should().BeNull();
        query.MaxResults.Should().Be(100);
        query.After.Should().BeNull();
        query.Before.Should().BeNull();
        query.Duration.Should().BeNull();
        query.RelatedEventId.Should().BeNull();
    }

    [Fact]
    public void TemporalQuery_CustomValues_SetCorrectly()
    {
        // Arrange
        var relatedId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var query = new TemporalQuery(
            StartAfter: now.AddDays(-7),
            EventType: "login",
            MaxResults: 50,
            RelatedEventId: relatedId);

        // Assert
        query.StartAfter.Should().Be(now.AddDays(-7));
        query.EventType.Should().Be("login");
        query.MaxResults.Should().Be(50);
        query.RelatedEventId.Should().Be(relatedId);
    }

    [Fact]
    public void TemporalQueryDefaults_DefaultMaxResults_Is100()
    {
        // Assert
        TemporalQueryDefaults.DefaultMaxResults.Should().Be(100);
    }

    [Fact]
    public void Timeline_AllPropertiesSet()
    {
        // Arrange
        var earliest = DateTime.UtcNow.AddHours(-2);
        var latest = DateTime.UtcNow;
        var evt1 = CreateEvent("login", "User login", earliest);
        var evt2 = CreateEvent("logout", "User logout", latest);
        var events = new List<TemporalEvent> { evt1, evt2 };
        var relations = new List<TemporalRelationEdge>
        {
            new TemporalRelationEdge(evt1, evt2, TemporalRelationType.Before, 1.0)
        };
        var eventsByType = new Dictionary<string, IReadOnlyList<TemporalEvent>>
        {
            ["login"] = new List<TemporalEvent> { evt1 },
            ["logout"] = new List<TemporalEvent> { evt2 }
        };

        // Act
        var timeline = new Timeline(events, relations, earliest, latest, eventsByType);

        // Assert
        timeline.Events.Should().HaveCount(2);
        timeline.Relations.Should().HaveCount(1);
        timeline.EarliestTime.Should().Be(earliest);
        timeline.LatestTime.Should().Be(latest);
        timeline.EventsByType.Should().HaveCount(2);
    }
}
