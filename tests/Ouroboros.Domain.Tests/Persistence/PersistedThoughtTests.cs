using Ouroboros.Domain.Persistence;

namespace Ouroboros.Tests.Persistence;

[Trait("Category", "Unit")]
public class PersistedThoughtTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var thought = new PersistedThought
        {
            Id = id,
            Type = "Observation",
            Content = "The sky is blue",
            Timestamp = timestamp
        };

        thought.Id.Should().Be(id);
        thought.Type.Should().Be("Observation");
        thought.Content.Should().Be("The sky is blue");
        thought.Timestamp.Should().Be(timestamp);
        thought.Confidence.Should().Be(0);
        thought.Relevance.Should().Be(0);
        thought.Origin.Should().Be("Reactive");
        thought.Priority.Should().Be("Normal");
        thought.ParentThoughtId.Should().BeNull();
        thought.TriggeringTrait.Should().BeNull();
        thought.Topic.Should().BeNull();
        thought.Tags.Should().BeNull();
        thought.MetadataJson.Should().BeNull();
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var id = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var timestamp = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var tags = new[] { "ai", "reasoning" };

        var thought = new PersistedThought
        {
            Id = id,
            Type = "Analytical",
            Content = "Deep analysis",
            Confidence = 0.95,
            Relevance = 0.8,
            Timestamp = timestamp,
            Origin = "Autonomous",
            Priority = "High",
            ParentThoughtId = parentId,
            TriggeringTrait = "Curiosity",
            Topic = "Machine Learning",
            Tags = tags,
            MetadataJson = "{\"key\":\"value\"}"
        };

        thought.Id.Should().Be(id);
        thought.Type.Should().Be("Analytical");
        thought.Confidence.Should().Be(0.95);
        thought.Relevance.Should().Be(0.8);
        thought.Origin.Should().Be("Autonomous");
        thought.Priority.Should().Be("High");
        thought.ParentThoughtId.Should().Be(parentId);
        thought.TriggeringTrait.Should().Be("Curiosity");
        thought.Topic.Should().Be("Machine Learning");
        thought.Tags.Should().BeEquivalentTo(tags);
        thought.MetadataJson.Should().Be("{\"key\":\"value\"}");
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;

        var t1 = new PersistedThought { Id = id, Type = "Obs", Content = "C", Timestamp = ts };
        var t2 = new PersistedThought { Id = id, Type = "Obs", Content = "C", Timestamp = ts };

        t1.Should().Be(t2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Observation",
            Content = "Original",
            Timestamp = DateTime.UtcNow
        };

        var modified = thought with { Content = "Modified", Confidence = 0.9 };

        modified.Content.Should().Be("Modified");
        modified.Confidence.Should().Be(0.9);
        modified.Id.Should().Be(thought.Id);
        thought.Content.Should().Be("Original");
    }

    [Fact]
    public void Tags_CanBeSetToArray()
    {
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Obs",
            Content = "Tagged",
            Timestamp = DateTime.UtcNow,
            Tags = new[] { "tag1", "tag2", "tag3" }
        };

        thought.Tags.Should().HaveCount(3);
        thought.Tags.Should().Contain("tag2");
    }
}
