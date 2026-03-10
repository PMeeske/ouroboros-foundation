// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Vectors;

using Ouroboros.Domain.Vectors;

/// <summary>
/// Tests for <see cref="MemoryStatistics"/>.
/// </summary>
[Trait("Category", "Unit")]
public class MemoryStatisticsTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var distribution = new Dictionary<ulong, int> { [768UL] = 3, [1536UL] = 2 };

        // Act
        var stats = new MemoryStatistics(
            TotalCollections: 5,
            TotalVectors: 1000L,
            HealthyCollections: 4,
            UnhealthyCollections: 1,
            CollectionLinks: 3,
            DimensionDistribution: distribution);

        // Assert
        stats.TotalCollections.Should().Be(5);
        stats.TotalVectors.Should().Be(1000L);
        stats.HealthyCollections.Should().Be(4);
        stats.UnhealthyCollections.Should().Be(1);
        stats.CollectionLinks.Should().Be(3);
        stats.DimensionDistribution.Should().HaveCount(2);
        stats.DimensionDistribution[768UL].Should().Be(3);
        stats.DimensionDistribution[1536UL].Should().Be(2);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var distribution = new Dictionary<ulong, int> { [768UL] = 1 };

        // Act
        var stats1 = new MemoryStatistics(1, 100L, 1, 0, 0, distribution);
        var stats2 = new MemoryStatistics(1, 100L, 1, 0, 0, distribution);

        // Assert
        stats1.Should().Be(stats2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var distribution = new Dictionary<ulong, int>();

        // Act
        var stats1 = new MemoryStatistics(1, 100L, 1, 0, 0, distribution);
        var stats2 = new MemoryStatistics(2, 200L, 2, 0, 0, distribution);

        // Assert
        stats1.Should().NotBe(stats2);
    }

    [Fact]
    public void Constructor_EmptyDistribution_IsValid()
    {
        // Act
        var stats = new MemoryStatistics(0, 0L, 0, 0, 0, new Dictionary<ulong, int>());

        // Assert
        stats.DimensionDistribution.Should().BeEmpty();
        stats.TotalCollections.Should().Be(0);
        stats.TotalVectors.Should().Be(0L);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        // Arrange
        var distribution = new Dictionary<ulong, int> { [768UL] = 1 };
        var original = new MemoryStatistics(1, 100L, 1, 0, 0, distribution);

        // Act
        var modified = original with { TotalVectors = 200L };

        // Assert
        modified.TotalVectors.Should().Be(200L);
        modified.TotalCollections.Should().Be(1);
        original.TotalVectors.Should().Be(100L);
    }
}
