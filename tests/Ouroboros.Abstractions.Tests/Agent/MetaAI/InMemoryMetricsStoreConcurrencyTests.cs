using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;
using AgentPerfMetrics = Ouroboros.Agent.PerformanceMetrics;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

/// <summary>
/// Additional tests for InMemoryMetricsStore covering concurrency,
/// edge cases, and cancellation token behavior.
/// </summary>
[Trait("Category", "Unit")]
public class InMemoryMetricsStoreConcurrencyTests
{
    private static AgentPerfMetrics CreateMetrics(
        string name = "resource-1",
        int executionCount = 10,
        double successRate = 0.9) =>
        new AgentPerfMetrics(
            name, executionCount, 50.0, successRate,
            DateTime.UtcNow, new Dictionary<string, double>());

    [Fact]
    public async Task ConcurrentStoreAndRetrieve_DoesNotThrow()
    {
        // Arrange
        var store = new InMemoryMetricsStore();

        // Act - concurrent writes
        var tasks = Enumerable.Range(0, 100)
            .Select(i => store.StoreMetricsAsync(CreateMetrics($"resource-{i}", i)))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var all = await store.GetAllMetricsAsync();
        all.Should().HaveCount(100);
    }

    [Fact]
    public async Task ConcurrentStoreAndRemove_DoesNotThrow()
    {
        // Arrange
        var store = new InMemoryMetricsStore();

        // Pre-populate
        for (int i = 0; i < 50; i++)
        {
            await store.StoreMetricsAsync(CreateMetrics($"r-{i}"));
        }

        // Act - concurrent removes and stores
        var removeTasks = Enumerable.Range(0, 25)
            .Select(i => store.RemoveMetricsAsync($"r-{i}"));
        var storeTasks = Enumerable.Range(50, 25)
            .Select(i => store.StoreMetricsAsync(CreateMetrics($"r-{i}")));

        await Task.WhenAll(removeTasks.Concat(storeTasks));

        // Assert - should not throw
        var all = await store.GetAllMetricsAsync();
        all.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMetricsAsync_WithCancellationToken_ReturnsResult()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var store = new InMemoryMetricsStore();
        await store.StoreMetricsAsync(CreateMetrics("r1"));

        // Act
        var result = await store.GetMetricsAsync("r1", cts.Token);

        // Assert
        result.Should().NotBeNull();
        result!.ResourceName.Should().Be("r1");
    }

    [Fact]
    public async Task StoreMetricsAsync_WithCancellationToken_Completes()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var store = new InMemoryMetricsStore();

        // Act
        await store.StoreMetricsAsync(CreateMetrics("r1"), cts.Token);
        var result = await store.GetMetricsAsync("r1");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveMetricsAsync_WithCancellationToken_Works()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var store = new InMemoryMetricsStore();
        await store.StoreMetricsAsync(CreateMetrics("r1"));

        // Act
        var removed = await store.RemoveMetricsAsync("r1", cts.Token);

        // Assert
        removed.Should().BeTrue();
    }

    [Fact]
    public async Task ClearAsync_WithCancellationToken_Works()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var store = new InMemoryMetricsStore();
        await store.StoreMetricsAsync(CreateMetrics("r1"));

        // Act
        await store.ClearAsync(cts.Token);
        var all = await store.GetAllMetricsAsync();

        // Assert
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllMetricsAsync_ReturnsDefensiveCopy()
    {
        // Arrange
        var store = new InMemoryMetricsStore();
        await store.StoreMetricsAsync(CreateMetrics("r1"));

        // Act
        var all1 = await store.GetAllMetricsAsync();
        await store.StoreMetricsAsync(CreateMetrics("r2"));
        var all2 = await store.GetAllMetricsAsync();

        // Assert - first snapshot should not have changed
        all1.Should().HaveCount(1);
        all2.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetStatisticsAsync_SingleMetric_ReturnsCorrectValues()
    {
        // Arrange
        var store = new InMemoryMetricsStore();
        await store.StoreMetricsAsync(new AgentPerfMetrics(
            "r1", 100, 25.0, 0.95, DateTime.UtcNow,
            new Dictionary<string, double> { ["tokens"] = 5000 }));

        // Act
        var stats = await store.GetStatisticsAsync();

        // Assert
        stats.TotalResources.Should().Be(1);
        stats.TotalExecutions.Should().Be(100);
        stats.OverallSuccessRate.Should().Be(0.95);
        stats.AverageLatencyMs.Should().Be(25.0);
    }

    [Fact]
    public async Task StoreMetricsAsync_RapidOverwrites_PreservesLastValue()
    {
        // Arrange
        var store = new InMemoryMetricsStore();

        // Act - rapidly overwrite the same key
        for (int i = 0; i < 100; i++)
        {
            await store.StoreMetricsAsync(CreateMetrics("same-key", executionCount: i));
        }

        // Assert
        var result = await store.GetMetricsAsync("same-key");
        result.Should().NotBeNull();
        result!.ExecutionCount.Should().Be(99);
    }
}
