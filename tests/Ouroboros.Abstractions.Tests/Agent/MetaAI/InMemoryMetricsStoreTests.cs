using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;
using AgentPerfMetrics = Ouroboros.Agent.PerformanceMetrics;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class InMemoryMetricsStoreTests
{
    private static AgentPerfMetrics CreateMetrics(
        string name = "resource-1",
        int executionCount = 10,
        double successRate = 0.9) =>
        new AgentPerfMetrics(
            name, executionCount, 50.0, successRate,
            DateTime.UtcNow, new Dictionary<string, double>());

    [Fact]
    public async Task StoreMetricsAsync_StoresMetrics()
    {
        // Arrange
        var store = new InMemoryMetricsStore();
        var metrics = CreateMetrics();

        // Act
        await store.StoreMetricsAsync(metrics);
        var retrieved = await store.GetMetricsAsync("resource-1");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.ResourceName.Should().Be("resource-1");
    }

    [Fact]
    public async Task StoreMetricsAsync_NullMetrics_ThrowsArgumentNullException()
    {
        // Arrange
        var store = new InMemoryMetricsStore();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => store.StoreMetricsAsync(null!));
    }

    [Fact]
    public async Task StoreMetricsAsync_SameResourceName_OverwritesPrevious()
    {
        // Arrange
        var store = new InMemoryMetricsStore();
        var metrics1 = CreateMetrics(executionCount: 5);
        var metrics2 = CreateMetrics(executionCount: 10);

        // Act
        await store.StoreMetricsAsync(metrics1);
        await store.StoreMetricsAsync(metrics2);
        var retrieved = await store.GetMetricsAsync("resource-1");

        // Assert
        retrieved!.ExecutionCount.Should().Be(10);
    }

    [Fact]
    public async Task GetMetricsAsync_NonExistentResource_ReturnsNull()
    {
        // Arrange
        var store = new InMemoryMetricsStore();

        // Act
        var result = await store.GetMetricsAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllMetricsAsync_ReturnsAllStoredMetrics()
    {
        // Arrange
        var store = new InMemoryMetricsStore();
        await store.StoreMetricsAsync(CreateMetrics("r1"));
        await store.StoreMetricsAsync(CreateMetrics("r2"));
        await store.StoreMetricsAsync(CreateMetrics("r3"));

        // Act
        var all = await store.GetAllMetricsAsync();

        // Assert
        all.Should().HaveCount(3);
        all.Keys.Should().Contain("r1");
        all.Keys.Should().Contain("r2");
        all.Keys.Should().Contain("r3");
    }

    [Fact]
    public async Task GetAllMetricsAsync_EmptyStore_ReturnsEmptyDictionary()
    {
        // Arrange
        var store = new InMemoryMetricsStore();

        // Act
        var all = await store.GetAllMetricsAsync();

        // Assert
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveMetricsAsync_ExistingResource_ReturnsTrue()
    {
        // Arrange
        var store = new InMemoryMetricsStore();
        await store.StoreMetricsAsync(CreateMetrics("r1"));

        // Act
        var removed = await store.RemoveMetricsAsync("r1");

        // Assert
        removed.Should().BeTrue();
        var retrieved = await store.GetMetricsAsync("r1");
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task RemoveMetricsAsync_NonExistentResource_ReturnsFalse()
    {
        // Arrange
        var store = new InMemoryMetricsStore();

        // Act
        var removed = await store.RemoveMetricsAsync("nonexistent");

        // Assert
        removed.Should().BeFalse();
    }

    [Fact]
    public async Task ClearAsync_RemovesAllMetrics()
    {
        // Arrange
        var store = new InMemoryMetricsStore();
        await store.StoreMetricsAsync(CreateMetrics("r1"));
        await store.StoreMetricsAsync(CreateMetrics("r2"));

        // Act
        await store.ClearAsync();
        var all = await store.GetAllMetricsAsync();

        // Assert
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStatisticsAsync_EmptyStore_ReturnsZeroStatistics()
    {
        // Arrange
        var store = new InMemoryMetricsStore();

        // Act
        var stats = await store.GetStatisticsAsync();

        // Assert
        stats.TotalResources.Should().Be(0);
        stats.TotalExecutions.Should().Be(0);
        stats.OverallSuccessRate.Should().Be(0);
        stats.AverageLatencyMs.Should().Be(0);
        stats.OldestMetric.Should().BeNull();
        stats.NewestMetric.Should().BeNull();
    }

    [Fact]
    public async Task GetStatisticsAsync_WithMetrics_ReturnsCorrectStatistics()
    {
        // Arrange
        var store = new InMemoryMetricsStore();
        await store.StoreMetricsAsync(CreateMetrics("r1", 10, 0.8));
        await store.StoreMetricsAsync(CreateMetrics("r2", 20, 1.0));

        // Act
        var stats = await store.GetStatisticsAsync();

        // Assert
        stats.TotalResources.Should().Be(2);
        stats.TotalExecutions.Should().Be(30);
        stats.OverallSuccessRate.Should().Be(0.9);
        stats.OldestMetric.Should().NotBeNull();
        stats.NewestMetric.Should().NotBeNull();
    }
}
