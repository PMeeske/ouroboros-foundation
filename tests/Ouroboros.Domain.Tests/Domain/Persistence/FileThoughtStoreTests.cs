// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Persistence;

using Ouroboros.Domain.Persistence;

/// <summary>
/// Tests for <see cref="FileThoughtStore"/> using temp directories.
/// </summary>
[Trait("Category", "Unit")]
public class FileThoughtStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileThoughtStore _sut;

    public FileThoughtStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ouroboros-thoughts-" + Guid.NewGuid().ToString("N")[..8]);
        _sut = new FileThoughtStore(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch (IOException) { }
    }

    private static PersistedThought CreateThought(
        string content = "Test thought",
        string type = "Observation",
        string origin = "Reactive",
        double confidence = 0.8,
        double relevance = 0.7,
        string? topic = null,
        string[]? tags = null,
        Guid? parentId = null)
    {
        return new PersistedThought
        {
            Id = Guid.NewGuid(),
            Content = content,
            Type = type,
            Timestamp = DateTime.UtcNow,
            Origin = origin,
            Confidence = confidence,
            Relevance = relevance,
            Topic = topic,
            Tags = tags,
            ParentThoughtId = parentId,
        };
    }

    // ----------------------------------------------------------------
    // Constructor
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_CreatesBaseDirectory()
    {
        Directory.Exists(_tempDir).Should().BeTrue();
    }

    [Fact]
    public void Constructor_NullDirectory_UsesDefault()
    {
        // Act - should not throw
        var store = new FileThoughtStore(null);
        store.Should().NotBeNull();
    }

    // ----------------------------------------------------------------
    // SaveThoughtAsync / GetThoughtsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task SaveAndGetThought_RoundTrip()
    {
        // Arrange
        var thought = CreateThought("Hello world");

        // Act
        await _sut.SaveThoughtAsync("session1", thought);
        IReadOnlyList<PersistedThought> thoughts = await _sut.GetThoughtsAsync("session1");

        // Assert
        thoughts.Should().HaveCount(1);
        thoughts[0].Content.Should().Be("Hello world");
    }

    [Fact]
    public async Task SaveThoughtsAsync_MultipleSaved()
    {
        // Arrange
        var thoughts = new[]
        {
            CreateThought("T1"),
            CreateThought("T2"),
            CreateThought("T3"),
        };

        // Act
        await _sut.SaveThoughtsAsync("session1", thoughts);
        IReadOnlyList<PersistedThought> result = await _sut.GetThoughtsAsync("session1");

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetThoughtsAsync_NonExistentSession_ReturnsEmpty()
    {
        // Act
        IReadOnlyList<PersistedThought> thoughts = await _sut.GetThoughtsAsync("nonexistent");

        // Assert
        thoughts.Should().BeEmpty();
    }

    // ----------------------------------------------------------------
    // GetThoughtsInRangeAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetThoughtsInRangeAsync_FiltersByTimestamp()
    {
        // Arrange
        var thought1 = CreateThought("Old");
        var thought2 = CreateThought("New");
        await _sut.SaveThoughtsAsync("session1", new[] { thought1, thought2 });

        DateTime from = DateTime.UtcNow.AddHours(-1);
        DateTime to = DateTime.UtcNow.AddHours(1);

        // Act
        IReadOnlyList<PersistedThought> result = await _sut.GetThoughtsInRangeAsync("session1", from, to);

        // Assert
        result.Should().HaveCount(2);
    }

    // ----------------------------------------------------------------
    // GetThoughtsByTypeAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetThoughtsByTypeAsync_FiltersByType()
    {
        // Arrange
        await _sut.SaveThoughtAsync("session1", CreateThought(type: "Observation"));
        await _sut.SaveThoughtAsync("session1", CreateThought(type: "Analytical"));
        await _sut.SaveThoughtAsync("session1", CreateThought(type: "Observation"));

        // Act
        IReadOnlyList<PersistedThought> result = await _sut.GetThoughtsByTypeAsync("session1", "Observation");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetThoughtsByTypeAsync_CaseInsensitive()
    {
        // Arrange
        await _sut.SaveThoughtAsync("session1", CreateThought(type: "Observation"));

        // Act
        IReadOnlyList<PersistedThought> result = await _sut.GetThoughtsByTypeAsync("session1", "observation");

        // Assert
        result.Should().HaveCount(1);
    }

    // ----------------------------------------------------------------
    // SearchThoughtsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task SearchThoughtsAsync_FindsByContent()
    {
        // Arrange
        await _sut.SaveThoughtAsync("session1", CreateThought("quantum computing is fascinating"));
        await _sut.SaveThoughtAsync("session1", CreateThought("the weather is nice"));

        // Act
        IReadOnlyList<PersistedThought> result = await _sut.SearchThoughtsAsync("session1", "quantum");

        // Assert
        result.Should().HaveCount(1);
        result[0].Content.Should().Contain("quantum");
    }

    [Fact]
    public async Task SearchThoughtsAsync_FindsByTopic()
    {
        // Arrange
        await _sut.SaveThoughtAsync("session1", CreateThought("some content", topic: "physics"));
        await _sut.SaveThoughtAsync("session1", CreateThought("other content", topic: "cooking"));

        // Act
        IReadOnlyList<PersistedThought> result = await _sut.SearchThoughtsAsync("session1", "physics");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchThoughtsAsync_FindsByTag()
    {
        // Arrange
        await _sut.SaveThoughtAsync("session1", CreateThought("content", tags: new[] { "ai", "science" }));
        await _sut.SaveThoughtAsync("session1", CreateThought("other", tags: new[] { "cooking" }));

        // Act
        IReadOnlyList<PersistedThought> result = await _sut.SearchThoughtsAsync("session1", "science");

        // Assert
        result.Should().HaveCount(1);
    }

    // ----------------------------------------------------------------
    // GetRecentThoughtsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetRecentThoughtsAsync_ReturnsLimitedResults()
    {
        // Arrange
        for (int i = 0; i < 20; i++)
        {
            await _sut.SaveThoughtAsync("session1", CreateThought($"Thought {i}"));
        }

        // Act
        IReadOnlyList<PersistedThought> result = await _sut.GetRecentThoughtsAsync("session1", 5);

        // Assert
        result.Should().HaveCount(5);
    }

    // ----------------------------------------------------------------
    // GetChainedThoughtsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetChainedThoughtsAsync_ReturnsChildren()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parent = CreateThought("Parent");
        var child = CreateThought("Child", parentId: parentId);

        // We need to set the parent Id manually
        var parentThought = new PersistedThought
        {
            Id = parentId,
            Content = "Parent",
            Type = "Observation",
            Timestamp = DateTime.UtcNow,
            Confidence = 0.8,
            Relevance = 0.7,
        };

        await _sut.SaveThoughtsAsync("session1", new[] { parentThought, child });

        // Act
        IReadOnlyList<PersistedThought> result = await _sut.GetChainedThoughtsAsync("session1", parentId);

        // Assert
        result.Should().HaveCount(1);
        result[0].Content.Should().Be("Child");
    }

    // ----------------------------------------------------------------
    // ClearSessionAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ClearSessionAsync_RemovesAllThoughts()
    {
        // Arrange
        await _sut.SaveThoughtAsync("session1", CreateThought("Test"));

        // Act
        await _sut.ClearSessionAsync("session1");
        IReadOnlyList<PersistedThought> result = await _sut.GetThoughtsAsync("session1");

        // Assert
        result.Should().BeEmpty();
    }

    // ----------------------------------------------------------------
    // GetStatisticsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetStatisticsAsync_EmptySession_ReturnsZeroCount()
    {
        // Act
        ThoughtStatistics stats = await _sut.GetStatisticsAsync("empty-session");

        // Assert
        stats.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithThoughts_ReturnsCorrectCounts()
    {
        // Arrange
        await _sut.SaveThoughtAsync("session1", CreateThought(type: "Observation"));
        await _sut.SaveThoughtAsync("session1", CreateThought(type: "Observation"));
        await _sut.SaveThoughtAsync("session1", CreateThought(type: "Analytical"));

        // Act
        ThoughtStatistics stats = await _sut.GetStatisticsAsync("session1");

        // Assert
        stats.TotalCount.Should().Be(3);
        stats.CountByType["Observation"].Should().Be(2);
        stats.CountByType["Analytical"].Should().Be(1);
    }

    // ----------------------------------------------------------------
    // ListSessionsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ListSessionsAsync_WithSessions_ReturnsList()
    {
        // Arrange
        await _sut.SaveThoughtAsync("session-a", CreateThought("A"));
        await _sut.SaveThoughtAsync("session-b", CreateThought("B"));

        // Act
        IReadOnlyList<string> sessions = await _sut.ListSessionsAsync();

        // Assert
        sessions.Should().HaveCount(2);
        sessions.Should().Contain("session-a");
        sessions.Should().Contain("session-b");
    }

    [Fact]
    public async Task ListSessionsAsync_NoSessions_ReturnsEmpty()
    {
        // Act
        IReadOnlyList<string> sessions = await _sut.ListSessionsAsync();

        // Assert
        sessions.Should().BeEmpty();
    }
}
