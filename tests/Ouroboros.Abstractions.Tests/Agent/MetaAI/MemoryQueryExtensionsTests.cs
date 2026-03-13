using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class MemoryQueryExtensionsTests
{
    [Fact]
    public void ForGoal_CreatesQueryWithGoal()
    {
        // Act
        var query = MemoryQueryExtensions.ForGoal("find patterns");

        // Assert
        query.Goal.Should().Be("find patterns");
        query.MaxResults.Should().Be(100);
        query.MinSimilarity.Should().Be(0.0);
    }

    [Fact]
    public void ForGoal_WithContext_ConvertsToString()
    {
        // Arrange
        var context = new Dictionary<string, object>
        {
            ["key1"] = "value1",
            ["key2"] = 42
        };

        // Act
        var query = MemoryQueryExtensions.ForGoal("goal", context, maxResults: 50);

        // Assert
        query.Goal.Should().Be("goal");
        query.MaxResults.Should().Be(50);
        query.Context.Should().Contain("key1=value1");
        query.ContextSimilarity.Should().NotBeNull();
    }

    [Fact]
    public void ForGoal_NullContext_SetsContextToNull()
    {
        // Act
        var query = MemoryQueryExtensions.ForGoal("goal", null);

        // Assert
        query.Context.Should().BeNull();
        query.ContextSimilarity.Should().BeNull();
    }

    [Fact]
    public void ForGoal_WithMinSimilarity_SetsThreshold()
    {
        // Act
        var query = MemoryQueryExtensions.ForGoal("goal", minSimilarity: 0.9);

        // Assert
        query.MinSimilarity.Should().Be(0.9);
    }

    [Fact]
    public void ForTags_CreatesQueryWithTags()
    {
        // Arrange
        var tags = new List<string> { "tag1", "tag2" };

        // Act
        var query = MemoryQueryExtensions.ForTags(tags);

        // Assert
        query.Tags.Should().HaveCount(2);
        query.Tags.Should().Contain("tag1");
        query.MaxResults.Should().Be(100);
    }

    [Fact]
    public void ForTags_WithMaxResults_SetsLimit()
    {
        // Act
        var query = MemoryQueryExtensions.ForTags(
            new List<string> { "tag" }, maxResults: 25);

        // Assert
        query.MaxResults.Should().Be(25);
    }

    [Fact]
    public void ForContext_CreatesQueryWithContextSimilarity()
    {
        // Act
        var query = MemoryQueryExtensions.ForContext("similar context");

        // Assert
        query.ContextSimilarity.Should().Be("similar context");
        query.Context.Should().Be("similar context");
        query.MinSimilarity.Should().Be(0.7);
        query.MaxResults.Should().Be(100);
    }

    [Fact]
    public void ForContext_WithCustomThreshold_SetsMinSimilarity()
    {
        // Act
        var query = MemoryQueryExtensions.ForContext("ctx", minSimilarity: 0.95);

        // Assert
        query.MinSimilarity.Should().Be(0.95);
    }

    [Fact]
    public void ForContext_WithCustomMaxResults_SetsLimit()
    {
        // Act
        var query = MemoryQueryExtensions.ForContext("ctx", maxResults: 5);

        // Assert
        query.MaxResults.Should().Be(5);
    }
}
