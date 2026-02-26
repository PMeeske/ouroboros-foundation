using FluentAssertions;
using Ouroboros.Domain.Persistence;
using Xunit;

namespace Ouroboros.Tests.Domain.Persistence;

[Trait("Category", "Unit")]
public class MemoryLayerTests
{
    [Theory]
    [InlineData(MemoryLayer.Working)]
    [InlineData(MemoryLayer.Episodic)]
    [InlineData(MemoryLayer.Semantic)]
    [InlineData(MemoryLayer.Procedural)]
    [InlineData(MemoryLayer.Autobiographical)]
    public void AllValues_AreDefined(MemoryLayer layer)
    {
        Enum.IsDefined(layer).Should().BeTrue();
    }

    [Fact]
    public void HasFiveValues()
    {
        Enum.GetValues<MemoryLayer>().Should().HaveCount(5);
    }
}

[Trait("Category", "Unit")]
public class PersistedThoughtTests
{
    [Fact]
    public void Create_ShouldSetRequiredFields()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var thought = new PersistedThought
        {
            Id = id,
            Type = "Observation",
            Content = "The sky is blue",
            Timestamp = now
        };

        thought.Id.Should().Be(id);
        thought.Type.Should().Be("Observation");
        thought.Content.Should().Be("The sky is blue");
        thought.Timestamp.Should().Be(now);
    }

    [Fact]
    public void Default_Confidence_ShouldBeZero()
    {
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Test",
            Content = "Test",
            Timestamp = DateTime.UtcNow
        };

        thought.Confidence.Should().Be(0.0);
    }

    [Fact]
    public void Default_Relevance_ShouldBeZero()
    {
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Test",
            Content = "Test",
            Timestamp = DateTime.UtcNow
        };

        thought.Relevance.Should().Be(0.0);
    }

    [Fact]
    public void Default_Origin_ShouldBeReactive()
    {
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Test",
            Content = "Test",
            Timestamp = DateTime.UtcNow
        };

        thought.Origin.Should().Be("Reactive");
    }

    [Fact]
    public void Default_Priority_ShouldBeNormal()
    {
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Test",
            Content = "Test",
            Timestamp = DateTime.UtcNow
        };

        thought.Priority.Should().Be("Normal");
    }

    [Fact]
    public void Default_ParentThoughtId_ShouldBeNull()
    {
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Test",
            Content = "Test",
            Timestamp = DateTime.UtcNow
        };

        thought.ParentThoughtId.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllOptionalFields_ShouldSetThem()
    {
        var parentId = Guid.NewGuid();
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Analytical",
            Content = "Deep analysis",
            Confidence = 0.85,
            Relevance = 0.9,
            Timestamp = DateTime.UtcNow,
            Origin = "Autonomous",
            Priority = "High",
            ParentThoughtId = parentId,
            TriggeringTrait = "Curiosity",
            Topic = "Mathematics",
            Tags = new[] { "math", "analysis" },
            MetadataJson = "{\"key\":\"value\"}"
        };

        thought.Confidence.Should().Be(0.85);
        thought.Relevance.Should().Be(0.9);
        thought.Origin.Should().Be("Autonomous");
        thought.Priority.Should().Be("High");
        thought.ParentThoughtId.Should().Be(parentId);
        thought.TriggeringTrait.Should().Be("Curiosity");
        thought.Topic.Should().Be("Mathematics");
        thought.Tags.Should().HaveCount(2);
        thought.MetadataJson.Should().Contain("key");
    }
}

[Trait("Category", "Unit")]
public class ThoughtRelationTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var id = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var relation = new ThoughtRelation(id, sourceId, targetId, "supports", 0.8, now);

        relation.Id.Should().Be(id);
        relation.SourceThoughtId.Should().Be(sourceId);
        relation.TargetThoughtId.Should().Be(targetId);
        relation.RelationType.Should().Be("supports");
        relation.Strength.Should().Be(0.8);
        relation.CreatedAt.Should().Be(now);
        relation.Metadata.Should().BeNull();
    }

    [Fact]
    public void Types_ShouldHaveCorrectConstants()
    {
        ThoughtRelation.Types.CausedBy.Should().Be("caused_by");
        ThoughtRelation.Types.LeadsTo.Should().Be("leads_to");
        ThoughtRelation.Types.Contradicts.Should().Be("contradicts");
        ThoughtRelation.Types.Supports.Should().Be("supports");
        ThoughtRelation.Types.Refines.Should().Be("refines");
        ThoughtRelation.Types.Abstracts.Should().Be("abstracts");
        ThoughtRelation.Types.Elaborates.Should().Be("elaborates");
        ThoughtRelation.Types.SimilarTo.Should().Be("similar_to");
        ThoughtRelation.Types.InstanceOf.Should().Be("instance_of");
        ThoughtRelation.Types.PartOf.Should().Be("part_of");
        ThoughtRelation.Types.Triggers.Should().Be("triggers");
        ThoughtRelation.Types.Resolves.Should().Be("resolves");
    }

    [Fact]
    public void Create_WithMetadata_ShouldSetIt()
    {
        var metadata = new Dictionary<string, object> { { "weight", 1.0 } };
        var relation = new ThoughtRelation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "supports", 0.9, DateTime.UtcNow, metadata);

        relation.Metadata.Should().NotBeNull();
        relation.Metadata!["weight"].Should().Be(1.0);
    }
}

[Trait("Category", "Unit")]
public class ThoughtResultTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var id = Guid.NewGuid();
        var thoughtId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var result = new ThoughtResult(id, thoughtId, "action", "Execute command", true, 0.95, now);

        result.Id.Should().Be(id);
        result.ThoughtId.Should().Be(thoughtId);
        result.ResultType.Should().Be("action");
        result.Content.Should().Be("Execute command");
        result.Success.Should().BeTrue();
        result.Confidence.Should().Be(0.95);
        result.CreatedAt.Should().Be(now);
        result.ExecutionTime.Should().BeNull();
        result.Metadata.Should().BeNull();
    }

    [Fact]
    public void Types_ShouldHaveCorrectConstants()
    {
        ThoughtResult.Types.Action.Should().Be("action");
        ThoughtResult.Types.Response.Should().Be("response");
        ThoughtResult.Types.Insight.Should().Be("insight");
        ThoughtResult.Types.Decision.Should().Be("decision");
        ThoughtResult.Types.SkillLearned.Should().Be("skill_learned");
        ThoughtResult.Types.FactDiscovered.Should().Be("fact_discovered");
        ThoughtResult.Types.Error.Should().Be("error");
        ThoughtResult.Types.Deferred.Should().Be("deferred");
    }
}

[Trait("Category", "Unit")]
public class ThoughtStatisticsTests
{
    [Fact]
    public void Default_ShouldHaveZeroValues()
    {
        var stats = new ThoughtStatistics();

        stats.TotalCount.Should().Be(0);
        stats.CountByType.Should().BeEmpty();
        stats.CountByOrigin.Should().BeEmpty();
        stats.AverageConfidence.Should().Be(0.0);
        stats.AverageRelevance.Should().Be(0.0);
        stats.EarliestThought.Should().BeNull();
        stats.LatestThought.Should().BeNull();
        stats.ChainCount.Should().Be(0);
    }

    [Fact]
    public void SetValues_ShouldPersist()
    {
        var earliest = DateTime.UtcNow.AddHours(-1);
        var latest = DateTime.UtcNow;

        var stats = new ThoughtStatistics
        {
            TotalCount = 100,
            CountByType = new Dictionary<string, int> { { "Observation", 50 }, { "Analytical", 50 } },
            CountByOrigin = new Dictionary<string, int> { { "Reactive", 80 }, { "Autonomous", 20 } },
            AverageConfidence = 0.75,
            AverageRelevance = 0.8,
            EarliestThought = earliest,
            LatestThought = latest,
            ChainCount = 10
        };

        stats.TotalCount.Should().Be(100);
        stats.CountByType.Should().HaveCount(2);
        stats.AverageConfidence.Should().Be(0.75);
        stats.ChainCount.Should().Be(10);
    }
}

[Trait("Category", "Unit")]
public class MemoryLayerMappingTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var mapping = new MemoryLayerMapping(
            Layer: MemoryLayer.Episodic,
            Collections: new[] { "conversations", "episodes" },
            Description: "Recent conversation history",
            RetentionPriority: 0.8);

        mapping.Layer.Should().Be(MemoryLayer.Episodic);
        mapping.Collections.Should().HaveCount(2);
        mapping.Description.Should().Be("Recent conversation history");
        mapping.RetentionPriority.Should().Be(0.8);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var collections = new[] { "col1" };
        var a = new MemoryLayerMapping(MemoryLayer.Working, collections, "desc", 1.0);
        var b = new MemoryLayerMapping(MemoryLayer.Working, collections, "desc", 1.0);
        a.Should().Be(b);
    }
}
