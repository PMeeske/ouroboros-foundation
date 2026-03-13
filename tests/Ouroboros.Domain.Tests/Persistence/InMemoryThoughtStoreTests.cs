using Ouroboros.Domain.Persistence;

namespace Ouroboros.Tests.Persistence;

[Trait("Category", "Unit")]
public class InMemoryThoughtStoreTests
{
    private readonly InMemoryThoughtStore _store = new();
    private const string SessionId = "test-session";

    private static PersistedThought CreateThought(
        string content = "Test thought",
        string type = "Observation",
        string origin = "Reactive",
        double confidence = 0.8,
        double relevance = 0.7,
        Guid? parentId = null,
        DateTime? timestamp = null,
        string? topic = null,
        string[]? tags = null) => new()
    {
        Id = Guid.NewGuid(),
        Type = type,
        Content = content,
        Confidence = confidence,
        Relevance = relevance,
        Origin = origin,
        Timestamp = timestamp ?? DateTime.UtcNow,
        ParentThoughtId = parentId,
        Topic = topic,
        Tags = tags
    };

    #region SaveThoughtAsync

    [Fact]
    public async Task SaveThoughtAsync_SingleThought_CanBeRetrieved()
    {
        var thought = CreateThought();

        await _store.SaveThoughtAsync(SessionId, thought);
        var result = await _store.GetThoughtsAsync(SessionId);

        result.Should().ContainSingle();
        result[0].Content.Should().Be("Test thought");
    }

    [Fact]
    public async Task SaveThoughtsAsync_MultipleThoughts_AllRetrieved()
    {
        var thoughts = new[]
        {
            CreateThought("Thought 1"),
            CreateThought("Thought 2"),
            CreateThought("Thought 3")
        };

        await _store.SaveThoughtsAsync(SessionId, thoughts);
        var result = await _store.GetThoughtsAsync(SessionId);

        result.Should().HaveCount(3);
    }

    #endregion

    #region GetThoughtsAsync

    [Fact]
    public async Task GetThoughtsAsync_NonExistentSession_ReturnsEmpty()
    {
        var result = await _store.GetThoughtsAsync("nonexistent");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetThoughtsAsync_ReturnsOrderedByTimestamp()
    {
        var old = CreateThought("Old", timestamp: DateTime.UtcNow.AddHours(-2));
        var recent = CreateThought("Recent", timestamp: DateTime.UtcNow);
        var middle = CreateThought("Middle", timestamp: DateTime.UtcNow.AddHours(-1));

        await _store.SaveThoughtsAsync(SessionId, new[] { old, recent, middle });
        var result = await _store.GetThoughtsAsync(SessionId);

        result[0].Content.Should().Be("Old");
        result[1].Content.Should().Be("Middle");
        result[2].Content.Should().Be("Recent");
    }

    #endregion

    #region GetThoughtsInRangeAsync

    [Fact]
    public async Task GetThoughtsInRangeAsync_FiltersCorrectly()
    {
        var baseTime = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        await _store.SaveThoughtsAsync(SessionId, new[]
        {
            CreateThought("Before", timestamp: baseTime.AddHours(-2)),
            CreateThought("InRange", timestamp: baseTime),
            CreateThought("After", timestamp: baseTime.AddHours(2))
        });

        var result = await _store.GetThoughtsInRangeAsync(
            SessionId,
            baseTime.AddHours(-1),
            baseTime.AddHours(1));

        result.Should().ContainSingle();
        result[0].Content.Should().Be("InRange");
    }

    #endregion

    #region GetThoughtsByTypeAsync

    [Fact]
    public async Task GetThoughtsByTypeAsync_FiltersByType()
    {
        await _store.SaveThoughtsAsync(SessionId, new[]
        {
            CreateThought("Obs1", type: "Observation"),
            CreateThought("Ana1", type: "Analytical"),
            CreateThought("Obs2", type: "observation") // case insensitive
        });

        var result = await _store.GetThoughtsByTypeAsync(SessionId, "Observation");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetThoughtsByTypeAsync_RespectsLimit()
    {
        await _store.SaveThoughtsAsync(SessionId, new[]
        {
            CreateThought("T1", type: "Observation"),
            CreateThought("T2", type: "Observation"),
            CreateThought("T3", type: "Observation")
        });

        var result = await _store.GetThoughtsByTypeAsync(SessionId, "Observation", limit: 2);

        result.Should().HaveCount(2);
    }

    #endregion

    #region SearchThoughtsAsync

    [Fact]
    public async Task SearchThoughtsAsync_FindsByContent()
    {
        await _store.SaveThoughtsAsync(SessionId, new[]
        {
            CreateThought("The quick brown fox"),
            CreateThought("Lazy dog sleeping"),
            CreateThought("Quick fox jumping")
        });

        var result = await _store.SearchThoughtsAsync(SessionId, "fox");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchThoughtsAsync_CaseInsensitive()
    {
        await _store.SaveThoughtAsync(SessionId, CreateThought("Hello World"));

        var result = await _store.SearchThoughtsAsync(SessionId, "hello");

        result.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchThoughtsAsync_RespectsLimit()
    {
        for (int i = 0; i < 10; i++)
        {
            await _store.SaveThoughtAsync(SessionId, CreateThought($"Match {i}"));
        }

        var result = await _store.SearchThoughtsAsync(SessionId, "Match", limit: 5);

        result.Should().HaveCount(5);
    }

    #endregion

    #region GetRecentThoughtsAsync

    [Fact]
    public async Task GetRecentThoughtsAsync_ReturnsMostRecent()
    {
        for (int i = 0; i < 5; i++)
        {
            await _store.SaveThoughtAsync(SessionId,
                CreateThought($"Thought {i}", timestamp: DateTime.UtcNow.AddMinutes(i)));
        }

        var result = await _store.GetRecentThoughtsAsync(SessionId, count: 2);

        result.Should().HaveCount(2);
        result[0].Content.Should().Be("Thought 4");
        result[1].Content.Should().Be("Thought 3");
    }

    #endregion

    #region GetChainedThoughtsAsync

    [Fact]
    public async Task GetChainedThoughtsAsync_FindsChildren()
    {
        var parentId = Guid.NewGuid();
        var parent = CreateThought("Parent") with { Id = parentId };
        var child1 = CreateThought("Child1", parentId: parentId);
        var child2 = CreateThought("Child2", parentId: parentId);
        var unrelated = CreateThought("Unrelated");

        await _store.SaveThoughtsAsync(SessionId, new[] { parent, child1, child2, unrelated });

        var result = await _store.GetChainedThoughtsAsync(SessionId, parentId);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(t => t.ParentThoughtId.Should().Be(parentId));
    }

    [Fact]
    public async Task GetChainedThoughtsAsync_NoChildren_ReturnsEmpty()
    {
        var parent = CreateThought("Lonely parent");
        await _store.SaveThoughtAsync(SessionId, parent);

        var result = await _store.GetChainedThoughtsAsync(SessionId, parent.Id);

        result.Should().BeEmpty();
    }

    #endregion

    #region ClearSessionAsync

    [Fact]
    public async Task ClearSessionAsync_RemovesAllThoughts()
    {
        await _store.SaveThoughtAsync(SessionId, CreateThought());
        await _store.SaveThoughtAsync(SessionId, CreateThought());

        await _store.ClearSessionAsync(SessionId);
        var result = await _store.GetThoughtsAsync(SessionId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearSessionAsync_DoesNotAffectOtherSessions()
    {
        await _store.SaveThoughtAsync("session-1", CreateThought());
        await _store.SaveThoughtAsync("session-2", CreateThought());

        await _store.ClearSessionAsync("session-1");

        var result1 = await _store.GetThoughtsAsync("session-1");
        var result2 = await _store.GetThoughtsAsync("session-2");

        result1.Should().BeEmpty();
        result2.Should().ContainSingle();
    }

    #endregion

    #region GetStatisticsAsync

    [Fact]
    public async Task GetStatisticsAsync_EmptySession_ReturnsZeroCount()
    {
        var stats = await _store.GetStatisticsAsync(SessionId);

        stats.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithThoughts_ReturnsCorrectStats()
    {
        await _store.SaveThoughtsAsync(SessionId, new[]
        {
            CreateThought("T1", type: "Observation", origin: "Reactive", confidence: 0.8, relevance: 0.9),
            CreateThought("T2", type: "Analytical", origin: "Autonomous", confidence: 0.6, relevance: 0.7),
            CreateThought("T3", type: "Observation", origin: "Reactive", confidence: 0.9, relevance: 0.8,
                parentId: Guid.NewGuid())
        });

        var stats = await _store.GetStatisticsAsync(SessionId);

        stats.TotalCount.Should().Be(3);
        stats.CountByType.Should().ContainKey("Observation").WhoseValue.Should().Be(2);
        stats.CountByType.Should().ContainKey("Analytical").WhoseValue.Should().Be(1);
        stats.CountByOrigin.Should().ContainKey("Reactive").WhoseValue.Should().Be(2);
        stats.AverageConfidence.Should().BeApproximately(0.7667, 0.01);
        stats.AverageRelevance.Should().BeApproximately(0.8, 0.01);
        stats.ChainCount.Should().Be(1);
    }

    #endregion

    #region ListSessionsAsync

    [Fact]
    public async Task ListSessionsAsync_ReturnsAllSessions()
    {
        await _store.SaveThoughtAsync("session-1", CreateThought());
        await _store.SaveThoughtAsync("session-2", CreateThought());

        var sessions = await _store.ListSessionsAsync();

        sessions.Should().HaveCount(2);
        sessions.Should().Contain("session-1");
        sessions.Should().Contain("session-2");
    }

    [Fact]
    public async Task ListSessionsAsync_NoSessions_ReturnsEmpty()
    {
        var sessions = await _store.ListSessionsAsync();

        sessions.Should().BeEmpty();
    }

    #endregion
}
