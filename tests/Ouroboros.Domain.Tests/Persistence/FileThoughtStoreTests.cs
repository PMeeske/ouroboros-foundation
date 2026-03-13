using Ouroboros.Domain.Persistence;

namespace Ouroboros.Tests.Persistence;

[Trait("Category", "Unit")]
public class FileThoughtStoreTests : IDisposable
{
    private readonly string _testDir;
    private readonly FileThoughtStore _store;
    private const string SessionId = "test-session";

    public FileThoughtStoreTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"ouroboros-test-{Guid.NewGuid():N}");
        _store = new FileThoughtStore(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    private static PersistedThought CreateThought(
        string content = "Test thought",
        string type = "Observation",
        DateTime? timestamp = null,
        Guid? parentId = null,
        string? topic = null,
        string[]? tags = null) => new()
    {
        Id = Guid.NewGuid(),
        Type = type,
        Content = content,
        Confidence = 0.8,
        Relevance = 0.7,
        Timestamp = timestamp ?? DateTime.UtcNow,
        ParentThoughtId = parentId,
        Topic = topic,
        Tags = tags
    };

    #region Constructor

    [Fact]
    public void Constructor_CreatesDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"ouroboros-ctor-test-{Guid.NewGuid():N}");
        try
        {
            _ = new FileThoughtStore(dir);

            Directory.Exists(dir).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    #endregion

    #region SaveAndRetrieve

    [Fact]
    public async Task SaveThoughtAsync_CanBeRetrieved()
    {
        var thought = CreateThought("Persisted thought");

        await _store.SaveThoughtAsync(SessionId, thought);
        var result = await _store.GetThoughtsAsync(SessionId);

        result.Should().ContainSingle();
        result[0].Content.Should().Be("Persisted thought");
    }

    [Fact]
    public async Task SaveThoughtsAsync_MultipleThoughts_AllPersisted()
    {
        var thoughts = new[] { CreateThought("T1"), CreateThought("T2") };

        await _store.SaveThoughtsAsync(SessionId, thoughts);
        var result = await _store.GetThoughtsAsync(SessionId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetThoughtsAsync_NonExistentSession_ReturnsEmpty()
    {
        var result = await _store.GetThoughtsAsync("nonexistent");

        result.Should().BeEmpty();
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

        var result = await _store.GetThoughtsInRangeAsync(SessionId, baseTime.AddHours(-1), baseTime.AddHours(1));

        result.Should().ContainSingle();
        result[0].Content.Should().Be("InRange");
    }

    #endregion

    #region GetThoughtsByTypeAsync

    [Fact]
    public async Task GetThoughtsByTypeAsync_FiltersByTypeCaseInsensitive()
    {
        await _store.SaveThoughtsAsync(SessionId, new[]
        {
            CreateThought("T1", type: "Observation"),
            CreateThought("T2", type: "Analytical"),
            CreateThought("T3", type: "observation")
        });

        var result = await _store.GetThoughtsByTypeAsync(SessionId, "Observation");

        result.Should().HaveCount(2);
    }

    #endregion

    #region SearchThoughtsAsync

    [Fact]
    public async Task SearchThoughtsAsync_SearchesByContent()
    {
        await _store.SaveThoughtsAsync(SessionId, new[]
        {
            CreateThought("The quick brown fox"),
            CreateThought("Lazy dog sleeping")
        });

        var result = await _store.SearchThoughtsAsync(SessionId, "fox");

        result.Should().ContainSingle();
        result[0].Content.Should().Contain("fox");
    }

    [Fact]
    public async Task SearchThoughtsAsync_SearchesByTopic()
    {
        await _store.SaveThoughtsAsync(SessionId, new[]
        {
            CreateThought("Something", topic: "machine learning"),
            CreateThought("Other thing", topic: "cooking")
        });

        var result = await _store.SearchThoughtsAsync(SessionId, "machine");

        result.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchThoughtsAsync_SearchesByTags()
    {
        await _store.SaveThoughtsAsync(SessionId, new[]
        {
            CreateThought("Tagged thought", tags: new[] { "ai", "ml" }),
            CreateThought("Other thought", tags: new[] { "cooking" })
        });

        var result = await _store.SearchThoughtsAsync(SessionId, "ai");

        result.Should().ContainSingle();
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
    }

    #endregion

    #region GetChainedThoughtsAsync

    [Fact]
    public async Task GetChainedThoughtsAsync_FindsChildrenRecursively()
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parent = CreateThought("Parent") with { Id = parentId };
        var child = CreateThought("Child", parentId: parentId) with { Id = childId };
        var grandchild = CreateThought("Grandchild", parentId: childId);

        await _store.SaveThoughtsAsync(SessionId, new[] { parent, child, grandchild });

        var result = await _store.GetChainedThoughtsAsync(SessionId, parentId);

        result.Should().HaveCount(2); // child + grandchild
    }

    #endregion

    #region ClearSessionAsync

    [Fact]
    public async Task ClearSessionAsync_DeletesFile()
    {
        await _store.SaveThoughtAsync(SessionId, CreateThought());

        await _store.ClearSessionAsync(SessionId);

        var result = await _store.GetThoughtsAsync(SessionId);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearSessionAsync_NonExistentSession_DoesNotThrow()
    {
        var act = () => _store.ClearSessionAsync("nonexistent");

        await act.Should().NotThrowAsync();
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
            CreateThought("T1", type: "Observation"),
            CreateThought("T2", type: "Analytical")
        });

        var stats = await _store.GetStatisticsAsync(SessionId);

        stats.TotalCount.Should().Be(2);
        stats.CountByType.Should().HaveCount(2);
    }

    #endregion

    #region ListSessionsAsync

    [Fact]
    public async Task ListSessionsAsync_ReturnsSessionIds()
    {
        await _store.SaveThoughtAsync("session-a", CreateThought());
        await _store.SaveThoughtAsync("session-b", CreateThought());

        var sessions = await _store.ListSessionsAsync();

        sessions.Should().HaveCount(2);
        sessions.Should().Contain("session-a");
        sessions.Should().Contain("session-b");
    }

    #endregion
}
