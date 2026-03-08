using FluentAssertions;
using Ouroboros.Domain.Persistence;
using Xunit;

namespace Ouroboros.Tests.Domain.Persistence;

[Trait("Category", "Unit")]
public class InMemoryThoughtStoreTests
{
    private readonly InMemoryThoughtStore _sut = new();

    private static PersistedThought CreateThought(
        string type = "Observation",
        string content = "Test thought",
        string origin = "Reactive",
        double confidence = 0.8,
        double relevance = 0.7,
        DateTime? timestamp = null,
        Guid? parentId = null)
    {
        return new PersistedThought
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = content,
            Origin = origin,
            Confidence = confidence,
            Relevance = relevance,
            Timestamp = timestamp ?? DateTime.UtcNow,
            ParentThoughtId = parentId
        };
    }

    // ===== SaveThoughtAsync =====

    [Fact]
    public async Task SaveThoughtAsync_ShouldPersistThought()
    {
        var thought = CreateThought();

        await _sut.SaveThoughtAsync("session1", thought);
        var thoughts = await _sut.GetThoughtsAsync("session1");

        thoughts.Should().ContainSingle();
        thoughts[0].Id.Should().Be(thought.Id);
    }

    // ===== SaveThoughtsAsync =====

    [Fact]
    public async Task SaveThoughtsAsync_ShouldPersistAll()
    {
        var thoughts = new[] { CreateThought(content: "A"), CreateThought(content: "B") };

        await _sut.SaveThoughtsAsync("session1", thoughts);
        var stored = await _sut.GetThoughtsAsync("session1");

        stored.Should().HaveCount(2);
    }

    // ===== GetThoughtsAsync =====

    [Fact]
    public async Task GetThoughtsAsync_NonExistentSession_ShouldReturnEmpty()
    {
        var thoughts = await _sut.GetThoughtsAsync("nonexistent");

        thoughts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetThoughtsAsync_ShouldReturnOrderedByTimestamp()
    {
        var older = CreateThought(content: "old", timestamp: DateTime.UtcNow.AddMinutes(-10));
        var newer = CreateThought(content: "new", timestamp: DateTime.UtcNow);

        await _sut.SaveThoughtAsync("s1", newer);
        await _sut.SaveThoughtAsync("s1", older);

        var thoughts = await _sut.GetThoughtsAsync("s1");

        thoughts[0].Content.Should().Be("old");
        thoughts[1].Content.Should().Be("new");
    }

    // ===== GetThoughtsInRangeAsync =====

    [Fact]
    public async Task GetThoughtsInRangeAsync_ShouldFilterByTimeRange()
    {
        var now = DateTime.UtcNow;
        await _sut.SaveThoughtAsync("s1", CreateThought(content: "before", timestamp: now.AddHours(-2)));
        await _sut.SaveThoughtAsync("s1", CreateThought(content: "within", timestamp: now.AddMinutes(-30)));
        await _sut.SaveThoughtAsync("s1", CreateThought(content: "after", timestamp: now.AddHours(2)));

        var thoughts = await _sut.GetThoughtsInRangeAsync("s1", now.AddHours(-1), now);

        thoughts.Should().ContainSingle();
        thoughts[0].Content.Should().Be("within");
    }

    // ===== GetThoughtsByTypeAsync =====

    [Fact]
    public async Task GetThoughtsByTypeAsync_ShouldFilterByType()
    {
        await _sut.SaveThoughtAsync("s1", CreateThought(type: "Observation"));
        await _sut.SaveThoughtAsync("s1", CreateThought(type: "Analytical"));
        await _sut.SaveThoughtAsync("s1", CreateThought(type: "Observation"));

        var thoughts = await _sut.GetThoughtsByTypeAsync("s1", "Observation");

        thoughts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetThoughtsByTypeAsync_ShouldBeCaseInsensitive()
    {
        await _sut.SaveThoughtAsync("s1", CreateThought(type: "Observation"));

        var thoughts = await _sut.GetThoughtsByTypeAsync("s1", "observation");

        thoughts.Should().ContainSingle();
    }

    [Fact]
    public async Task GetThoughtsByTypeAsync_ShouldRespectLimit()
    {
        for (int i = 0; i < 5; i++)
            await _sut.SaveThoughtAsync("s1", CreateThought(type: "Observation"));

        var thoughts = await _sut.GetThoughtsByTypeAsync("s1", "Observation", limit: 3);

        thoughts.Should().HaveCount(3);
    }

    // ===== SearchThoughtsAsync =====

    [Fact]
    public async Task SearchThoughtsAsync_ShouldFindMatchingContent()
    {
        await _sut.SaveThoughtAsync("s1", CreateThought(content: "The cat sat on the mat"));
        await _sut.SaveThoughtAsync("s1", CreateThought(content: "The dog ran in the park"));

        var results = await _sut.SearchThoughtsAsync("s1", "cat");

        results.Should().ContainSingle();
        results[0].Content.Should().Contain("cat");
    }

    [Fact]
    public async Task SearchThoughtsAsync_ShouldBeCaseInsensitive()
    {
        await _sut.SaveThoughtAsync("s1", CreateThought(content: "Machine Learning"));

        var results = await _sut.SearchThoughtsAsync("s1", "machine");

        results.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchThoughtsAsync_ShouldRespectLimit()
    {
        for (int i = 0; i < 30; i++)
            await _sut.SaveThoughtAsync("s1", CreateThought(content: $"Test item {i}"));

        var results = await _sut.SearchThoughtsAsync("s1", "Test", limit: 5);

        results.Should().HaveCount(5);
    }

    // ===== GetRecentThoughtsAsync =====

    [Fact]
    public async Task GetRecentThoughtsAsync_ShouldReturnMostRecent()
    {
        var now = DateTime.UtcNow;
        for (int i = 0; i < 10; i++)
            await _sut.SaveThoughtAsync("s1", CreateThought(
                content: $"Thought {i}",
                timestamp: now.AddMinutes(i)));

        var recent = await _sut.GetRecentThoughtsAsync("s1", count: 3);

        recent.Should().HaveCount(3);
        recent[0].Content.Should().Be("Thought 9");
    }

    // ===== GetChainedThoughtsAsync =====

    [Fact]
    public async Task GetChainedThoughtsAsync_ShouldReturnChildren()
    {
        var parentId = Guid.NewGuid();
        await _sut.SaveThoughtAsync("s1", CreateThought(content: "child1", parentId: parentId));
        await _sut.SaveThoughtAsync("s1", CreateThought(content: "child2", parentId: parentId));
        await _sut.SaveThoughtAsync("s1", CreateThought(content: "unrelated"));

        var chained = await _sut.GetChainedThoughtsAsync("s1", parentId);

        chained.Should().HaveCount(2);
    }

    // ===== ClearSessionAsync =====

    [Fact]
    public async Task ClearSessionAsync_ShouldRemoveAllThoughts()
    {
        await _sut.SaveThoughtAsync("s1", CreateThought());
        await _sut.SaveThoughtAsync("s1", CreateThought());

        await _sut.ClearSessionAsync("s1");
        var thoughts = await _sut.GetThoughtsAsync("s1");

        thoughts.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearSessionAsync_ShouldNotAffectOtherSessions()
    {
        await _sut.SaveThoughtAsync("s1", CreateThought());
        await _sut.SaveThoughtAsync("s2", CreateThought());

        await _sut.ClearSessionAsync("s1");

        var s1 = await _sut.GetThoughtsAsync("s1");
        var s2 = await _sut.GetThoughtsAsync("s2");

        s1.Should().BeEmpty();
        s2.Should().ContainSingle();
    }

    // ===== GetStatisticsAsync =====

    [Fact]
    public async Task GetStatisticsAsync_EmptySession_ShouldReturnZeroCount()
    {
        var stats = await _sut.GetStatisticsAsync("empty");

        stats.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithThoughts_ShouldComputeStats()
    {
        var parentId = Guid.NewGuid();
        await _sut.SaveThoughtAsync("s1", CreateThought(type: "Observation", confidence: 0.8, relevance: 0.6));
        await _sut.SaveThoughtAsync("s1", CreateThought(type: "Analytical", confidence: 0.9, relevance: 0.8, parentId: parentId));
        await _sut.SaveThoughtAsync("s1", CreateThought(type: "Observation", confidence: 0.7, relevance: 0.7));

        var stats = await _sut.GetStatisticsAsync("s1");

        stats.TotalCount.Should().Be(3);
        stats.CountByType.Should().ContainKey("Observation").WhoseValue.Should().Be(2);
        stats.CountByType.Should().ContainKey("Analytical").WhoseValue.Should().Be(1);
        stats.AverageConfidence.Should().BeApproximately(0.8, 0.01);
        stats.ChainCount.Should().Be(1);
    }

    // ===== ListSessionsAsync =====

    [Fact]
    public async Task ListSessionsAsync_ShouldReturnAllSessionIds()
    {
        await _sut.SaveThoughtAsync("s1", CreateThought());
        await _sut.SaveThoughtAsync("s2", CreateThought());

        var sessions = await _sut.ListSessionsAsync();

        sessions.Should().Contain("s1");
        sessions.Should().Contain("s2");
    }
}
