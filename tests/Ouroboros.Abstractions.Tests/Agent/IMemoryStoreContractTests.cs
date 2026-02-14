// <copyright file="IMemoryStoreContractTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Abstractions.Tests.Agent;

/// <summary>
/// Contract tests for IMemoryStore interface concept for Ouroboros.Abstractions.Agent namespace.
/// These tests verify that such an interface can be implemented standalone
/// without Engine dependencies, using test-local mock definitions.
/// </summary>
[Trait("Category", "Unit")]
public class IMemoryStoreContractTests
{
    [Fact]
    public void MemoryQuery_CanBeInstantiated()
    {
        // Arrange & Act
        var query = new MemoryQuery(
            "test search text",
            MaxResults: 10,
            MinRelevanceScore: 0.7);

        // Assert
        query.SearchText.Should().Be("test search text");
        query.MaxResults.Should().Be(10);
        query.MinRelevanceScore.Should().Be(0.7);
    }

    [Fact]
    public void MemoryQuery_DefaultValues_ShouldBeReasonable()
    {
        // Arrange & Act
        var query = new MemoryQuery("search");

        // Assert
        query.SearchText.Should().Be("search");
        query.MaxResults.Should().Be(10);
        query.MinRelevanceScore.Should().Be(0.5);
    }

    [Fact]
    public void MemoryStatistics_CanBeInstantiated()
    {
        // Arrange & Act
        var stats = new MemoryStatistics(
            TotalMemories: 1000,
            TotalVectors: 1000,
            MemorySizeBytes: 1024 * 1024,
            CollectionName: "test-collection");

        // Assert
        stats.TotalMemories.Should().Be(1000);
        stats.TotalVectors.Should().Be(1000);
        stats.MemorySizeBytes.Should().Be(1024 * 1024);
        stats.CollectionName.Should().Be("test-collection");
    }

    [Fact]
    public async Task FakeMemoryStore_CanBeImplemented()
    {
        // Arrange
        var store = new FakeMemoryStore();
        var query = new MemoryQuery("test", 5, 0.5);

        // Act
        var memories = await store.QueryMemoriesAsync(query, CancellationToken.None);
        var stats = await store.GetStatisticsAsync(CancellationToken.None);

        // Assert
        memories.Should().NotBeNull();
        stats.Should().NotBeNull();
        stats.CollectionName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task IMemoryStore_CanBeReferencedFromAbstractionsAgent()
    {
        // This test verifies a memory store interface concept can be implemented
        // without requiring Core or Engine dependencies
        
        // Arrange
        IMemoryStore store = new FakeMemoryStore();
        var query = new MemoryQuery("test");

        // Act
        var result = await store.QueryMemoriesAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void IMemoryStore_InterfaceExists()
    {
        // Verify the test-local interface type exists (demonstrating the concept)
        var interfaceType = typeof(IMemoryStore);
        
        interfaceType.Should().NotBeNull();
        interfaceType.IsInterface.Should().BeTrue();
    }

    [Fact]
    public async Task MemoryStore_StoreAndQuery_ShouldWork()
    {
        // Arrange
        var store = new FakeMemoryStore();
        var memoryContent = "Test memory content";

        // Act
        await store.StoreMemoryAsync(memoryContent, CancellationToken.None);
        var query = new MemoryQuery("memory", 10, 0.5);
        var results = await store.QueryMemoriesAsync(query, CancellationToken.None);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCountGreaterThanOrEqualTo(1);
    }
}

/// <summary>
/// Query parameters for memory search.
/// </summary>
/// <param name="SearchText">The text to search for.</param>
/// <param name="MaxResults">Maximum number of results to return.</param>
/// <param name="MinRelevanceScore">Minimum relevance score (0-1).</param>
public sealed record MemoryQuery(
    string SearchText,
    int MaxResults = 10,
    double MinRelevanceScore = 0.5);

/// <summary>
/// Statistics about the memory store.
/// </summary>
/// <param name="TotalMemories">Total number of memories stored.</param>
/// <param name="TotalVectors">Total number of vectors.</param>
/// <param name="MemorySizeBytes">Total size in bytes.</param>
/// <param name="CollectionName">Name of the collection.</param>
public sealed record MemoryStatistics(
    long TotalMemories,
    long TotalVectors,
    long MemorySizeBytes,
    string CollectionName);

/// <summary>
/// Interface for memory storage and retrieval.
/// </summary>
public interface IMemoryStore
{
    /// <summary>
    /// Stores a memory.
    /// </summary>
    /// <param name="content">The memory content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task StoreMemoryAsync(string content, CancellationToken cancellationToken);

    /// <summary>
    /// Queries memories based on search criteria.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching memory contents.</returns>
    Task<IReadOnlyList<string>> QueryMemoriesAsync(MemoryQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Gets statistics about the memory store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Memory statistics.</returns>
    Task<MemoryStatistics> GetStatisticsAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Fake implementation for testing purposes.
/// </summary>
internal sealed class FakeMemoryStore : IMemoryStore
{
    private readonly List<string> _memories = new();

    public Task StoreMemoryAsync(string content, CancellationToken cancellationToken)
    {
        _memories.Add(content);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> QueryMemoriesAsync(MemoryQuery query, CancellationToken cancellationToken)
    {
        var results = _memories
            .Where(m => m.Contains(query.SearchText, StringComparison.OrdinalIgnoreCase))
            .Take(query.MaxResults)
            .ToList();
        
        return Task.FromResult<IReadOnlyList<string>>(results);
    }

    public Task<MemoryStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new MemoryStatistics(
            TotalMemories: _memories.Count,
            TotalVectors: _memories.Count,
            MemorySizeBytes: _memories.Sum(m => m.Length * 2), // rough estimate
            CollectionName: "fake-collection"));
    }
}
