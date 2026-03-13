using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class MemoryQueryTests
{
    [Fact]
    public void MemoryQuery_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var query = new Ouroboros.Agent.MetaAI.MemoryQuery();

        // Assert
        query.Tags.Should().BeNull();
        query.ContextSimilarity.Should().BeNull();
        query.SuccessOnly.Should().BeNull();
        query.FromDate.Should().BeNull();
        query.ToDate.Should().BeNull();
        query.MaxResults.Should().Be(100);
        query.Goal.Should().BeNull();
        query.MinSimilarity.Should().Be(0.0);
        query.Context.Should().BeNull();
    }

    [Fact]
    public void MemoryQuery_WithAllParameters_SetsCorrectly()
    {
        // Arrange
        var tags = new List<string> { "tag1", "tag2" };
        var from = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var query = new Ouroboros.Agent.MetaAI.MemoryQuery(
            Tags: tags,
            ContextSimilarity: "similar context",
            SuccessOnly: true,
            FromDate: from,
            ToDate: to,
            MaxResults: 50,
            Goal: "find patterns",
            MinSimilarity: 0.8,
            Context: "current context");

        // Assert
        query.Tags.Should().HaveCount(2);
        query.ContextSimilarity.Should().Be("similar context");
        query.SuccessOnly.Should().BeTrue();
        query.FromDate.Should().Be(from);
        query.ToDate.Should().Be(to);
        query.MaxResults.Should().Be(50);
        query.Goal.Should().Be("find patterns");
        query.MinSimilarity.Should().Be(0.8);
        query.Context.Should().Be("current context");
    }

    [Fact]
    public void MemoryQuery_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new Ouroboros.Agent.MetaAI.MemoryQuery(Goal: "goal1", MaxResults: 10);
        var b = new Ouroboros.Agent.MetaAI.MemoryQuery(Goal: "goal1", MaxResults: 10);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void MemoryQuery_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new Ouroboros.Agent.MetaAI.MemoryQuery(Goal: "goal1", MaxResults: 10);

        // Act
        var modified = original with { MaxResults = 50 };

        // Assert
        modified.MaxResults.Should().Be(50);
        modified.Goal.Should().Be("goal1");
    }
}
