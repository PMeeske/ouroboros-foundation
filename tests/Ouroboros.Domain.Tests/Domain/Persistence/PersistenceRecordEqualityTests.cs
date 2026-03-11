using Ouroboros.Domain.Persistence;

namespace Ouroboros.Tests.Domain.Persistence;

[Trait("Category", "Unit")]
public class PersistenceRecordEqualityTests
{
    [Fact]
    public void PersistedThought_Equality_SameValues()
    {
        var id = Guid.NewGuid();
        var timestamp = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new PersistedThought { Id = id, Type = "Observation", Content = "Test", Timestamp = timestamp };
        var b = new PersistedThought { Id = id, Type = "Observation", Content = "Test", Timestamp = timestamp };

        a.Should().Be(b);
    }

    [Fact]
    public void PersistedThought_Equality_DifferentId_NotEqual()
    {
        var timestamp = DateTime.UtcNow;

        var a = new PersistedThought { Id = Guid.NewGuid(), Type = "Observation", Content = "Test", Timestamp = timestamp };
        var b = new PersistedThought { Id = Guid.NewGuid(), Type = "Observation", Content = "Test", Timestamp = timestamp };

        a.Should().NotBe(b);
    }

    [Fact]
    public void PersistedThought_WithExpression_ChangesContent()
    {
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Observation",
            Content = "Original",
            Timestamp = DateTime.UtcNow,
        };

        var modified = thought with { Content = "Modified" };

        modified.Content.Should().Be("Modified");
        thought.Content.Should().Be("Original");
    }

    [Fact]
    public void PersistedThought_WithExpression_ChangesConfidence()
    {
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Test",
            Content = "Test",
            Timestamp = DateTime.UtcNow,
            Confidence = 0.5,
        };

        var modified = thought with { Confidence = 0.95 };

        modified.Confidence.Should().Be(0.95);
        thought.Confidence.Should().Be(0.5);
    }

    [Fact]
    public void PersistedThought_WithExpression_AddsTags()
    {
        var thought = new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = "Test",
            Content = "Test",
            Timestamp = DateTime.UtcNow,
        };

        var modified = thought with { Tags = new[] { "tag1", "tag2" } };

        modified.Tags.Should().HaveCount(2);
        thought.Tags.Should().BeNull();
    }

    [Fact]
    public void ThoughtRelation_WithExpression_ChangesStrength()
    {
        var relation = new ThoughtRelation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "supports", 0.5, DateTime.UtcNow);

        var modified = relation with { Strength = 0.99 };

        modified.Strength.Should().Be(0.99);
        relation.Strength.Should().Be(0.5);
    }

    [Fact]
    public void ThoughtRelation_WithExpression_ChangesRelationType()
    {
        var relation = new ThoughtRelation(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            ThoughtRelation.Types.Supports, 0.8, DateTime.UtcNow);

        var modified = relation with { RelationType = ThoughtRelation.Types.Contradicts };

        modified.RelationType.Should().Be("contradicts");
        relation.RelationType.Should().Be("supports");
    }

    [Fact]
    public void ThoughtResult_WithExpression_ChangesSuccess()
    {
        var result = new ThoughtResult(
            Guid.NewGuid(), Guid.NewGuid(),
            ThoughtResult.Types.Action, "Execute", true, 0.9, DateTime.UtcNow);

        var modified = result with { Success = false };

        modified.Success.Should().BeFalse();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void ThoughtResult_WithExpression_AddsExecutionTime()
    {
        var result = new ThoughtResult(
            Guid.NewGuid(), Guid.NewGuid(),
            "action", "Execute", true, 0.9, DateTime.UtcNow);

        var modified = result with { ExecutionTime = TimeSpan.FromSeconds(2.5) };

        modified.ExecutionTime.Should().Be(TimeSpan.FromSeconds(2.5));
        result.ExecutionTime.Should().BeNull();
    }

    [Fact]
    public void ThoughtResult_WithExpression_AddsMetadata()
    {
        var result = new ThoughtResult(
            Guid.NewGuid(), Guid.NewGuid(),
            "action", "Execute", true, 0.9, DateTime.UtcNow);

        var metadata = new Dictionary<string, object> { ["key"] = "value" };
        var modified = result with { Metadata = metadata };

        modified.Metadata.Should().ContainKey("key");
        result.Metadata.Should().BeNull();
    }

    [Fact]
    public void ThoughtStatistics_WithExpression_ChangesTotalCount()
    {
        var stats = new ThoughtStatistics { TotalCount = 50 };
        var modified = stats with { TotalCount = 100 };

        modified.TotalCount.Should().Be(100);
        stats.TotalCount.Should().Be(50);
    }

    [Fact]
    public void ThoughtStatistics_Equality_SameValues()
    {
        var countByType = new Dictionary<string, int>();
        var countByOrigin = new Dictionary<string, int>();

        var a = new ThoughtStatistics
        {
            TotalCount = 10,
            CountByType = countByType,
            CountByOrigin = countByOrigin,
            AverageConfidence = 0.8,
            AverageRelevance = 0.7,
        };
        var b = new ThoughtStatistics
        {
            TotalCount = 10,
            CountByType = countByType,
            CountByOrigin = countByOrigin,
            AverageConfidence = 0.8,
            AverageRelevance = 0.7,
        };

        a.Should().Be(b);
    }

    [Fact]
    public void MemoryLayerMapping_WithExpression_ChangesLayer()
    {
        var mapping = new MemoryLayerMapping(
            MemoryLayer.Working, new[] { "col1" }, "Working memory", 1.0);

        var modified = mapping with { Layer = MemoryLayer.Semantic };

        modified.Layer.Should().Be(MemoryLayer.Semantic);
        mapping.Layer.Should().Be(MemoryLayer.Working);
    }

    [Fact]
    public void MemoryLayerMapping_WithExpression_ChangesRetentionPriority()
    {
        var mapping = new MemoryLayerMapping(
            MemoryLayer.Episodic, new[] { "col1" }, "Episodic", 0.5);

        var modified = mapping with { RetentionPriority = 0.9 };

        modified.RetentionPriority.Should().Be(0.9);
        mapping.RetentionPriority.Should().Be(0.5);
    }

    [Fact]
    public void MemoryLayer_CanParseFromString()
    {
        Enum.Parse<MemoryLayer>("Working").Should().Be(MemoryLayer.Working);
        Enum.Parse<MemoryLayer>("Autobiographical").Should().Be(MemoryLayer.Autobiographical);
    }
}
