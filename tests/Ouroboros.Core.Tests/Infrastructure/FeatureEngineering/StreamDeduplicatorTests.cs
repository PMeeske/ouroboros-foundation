// <copyright file="StreamDeduplicatorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Infrastructure.FeatureEngineering;

namespace Ouroboros.Tests.Infrastructure.FeatureEngineering;

[Trait("Category", "Unit")]
public class StreamDeduplicatorTests
{
    // --- Constructor validation ---

    [Fact]
    public void Constructor_DefaultParameters_ShouldNotThrow()
    {
        var act = () => new StreamDeduplicator();
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ValidParameters_ShouldNotThrow()
    {
        var act = () => new StreamDeduplicator(similarityThreshold: 0.9f, maxCacheSize: 500);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    [InlineData(-1.0f)]
    [InlineData(2.0f)]
    public void Constructor_InvalidSimilarityThreshold_ShouldThrow(float threshold)
    {
        var act = () => new StreamDeduplicator(similarityThreshold: threshold);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_BoundaryThreshold_Zero_ShouldNotThrow()
    {
        var act = () => new StreamDeduplicator(similarityThreshold: 0.0f);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_BoundaryThreshold_One_ShouldNotThrow()
    {
        var act = () => new StreamDeduplicator(similarityThreshold: 1.0f);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_InvalidMaxCacheSize_ShouldThrow(int cacheSize)
    {
        var act = () => new StreamDeduplicator(maxCacheSize: cacheSize);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_MinCacheSize_One_ShouldNotThrow()
    {
        var act = () => new StreamDeduplicator(maxCacheSize: 1);
        act.Should().NotThrow();
    }

    // --- IsDuplicate ---

    [Fact]
    public void IsDuplicate_NullVector_ShouldThrow()
    {
        var deduplicator = new StreamDeduplicator();

        var act = () => deduplicator.IsDuplicate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsDuplicate_FirstVector_ShouldReturnFalse()
    {
        var deduplicator = new StreamDeduplicator();
        float[] vector = new[] { 1f, 0f, 0f };

        bool result = deduplicator.IsDuplicate(vector);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsDuplicate_IdenticalVector_ShouldReturnTrue()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 0.99f);
        float[] vector = new[] { 1f, 0f, 0f };

        deduplicator.IsDuplicate(vector); // First: adds to cache
        bool isDuplicate = deduplicator.IsDuplicate(vector); // Second: detected as duplicate

        isDuplicate.Should().BeTrue();
    }

    [Fact]
    public void IsDuplicate_CompletelyDifferentVector_ShouldReturnFalse()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 0.99f);
        float[] vector1 = new[] { 1f, 0f, 0f };
        float[] vector2 = new[] { 0f, 1f, 0f };

        deduplicator.IsDuplicate(vector1);
        bool isDuplicate = deduplicator.IsDuplicate(vector2);

        isDuplicate.Should().BeFalse();
    }

    // --- CacheSize ---

    [Fact]
    public void CacheSize_Initially_ShouldBeZero()
    {
        var deduplicator = new StreamDeduplicator();

        deduplicator.CacheSize.Should().Be(0);
    }

    [Fact]
    public void CacheSize_AfterAddingUniqueVector_ShouldIncrement()
    {
        var deduplicator = new StreamDeduplicator();
        float[] vector = new[] { 1f, 0f, 0f };

        deduplicator.IsDuplicate(vector);

        deduplicator.CacheSize.Should().Be(1);
    }

    [Fact]
    public void CacheSize_AfterAddingDuplicate_ShouldNotIncrement()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 0.99f);
        float[] vector = new[] { 1f, 0f, 0f };

        deduplicator.IsDuplicate(vector);
        deduplicator.IsDuplicate(vector); // Duplicate

        deduplicator.CacheSize.Should().Be(1);
    }

    [Fact]
    public void CacheSize_ShouldNotExceedMaxCacheSize()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 1.0f, maxCacheSize: 2);

        // Add 3 orthogonal vectors (never duplicates of each other at threshold 1.0)
        deduplicator.IsDuplicate(new[] { 1f, 0f, 0f });
        deduplicator.IsDuplicate(new[] { 0f, 1f, 0f });
        deduplicator.IsDuplicate(new[] { 0f, 0f, 1f });

        deduplicator.CacheSize.Should().Be(2);
    }

    // --- ClearCache ---

    [Fact]
    public void ClearCache_ShouldResetCacheSize()
    {
        var deduplicator = new StreamDeduplicator();
        deduplicator.IsDuplicate(new[] { 1f, 0f, 0f });
        deduplicator.IsDuplicate(new[] { 0f, 1f, 0f });

        deduplicator.ClearCache();

        deduplicator.CacheSize.Should().Be(0);
    }

    [Fact]
    public void ClearCache_PreviouslySeenVector_ShouldNoLongerBeDuplicate()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 0.99f);
        float[] vector = new[] { 1f, 0f, 0f };

        deduplicator.IsDuplicate(vector); // Adds to cache
        deduplicator.ClearCache();
        bool isDuplicate = deduplicator.IsDuplicate(vector); // Should not be duplicate

        isDuplicate.Should().BeFalse();
    }

    // --- GetStatistics ---

    [Fact]
    public void GetStatistics_ShouldReturnCorrectValues()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 0.9f, maxCacheSize: 500);
        deduplicator.IsDuplicate(new[] { 1f, 0f });

        var (cacheSize, maxCacheSize, threshold) = deduplicator.GetStatistics();

        cacheSize.Should().Be(1);
        maxCacheSize.Should().Be(500);
        threshold.Should().Be(0.9f);
    }

    [Fact]
    public void GetStatistics_EmptyCache_ShouldReturnZeroCacheSize()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 0.95f, maxCacheSize: 1000);

        var (cacheSize, maxCacheSize, threshold) = deduplicator.GetStatistics();

        cacheSize.Should().Be(0);
        maxCacheSize.Should().Be(1000);
        threshold.Should().Be(0.95f);
    }

    // --- FilterBatch ---

    [Fact]
    public void FilterBatch_WithNull_ShouldThrow()
    {
        var deduplicator = new StreamDeduplicator();

        var act = () => deduplicator.FilterBatch(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FilterBatch_WithUniqueVectors_ShouldReturnAll()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 1.0f);
        var vectors = new List<float[]>
        {
            new[] { 1f, 0f, 0f },
            new[] { 0f, 1f, 0f },
            new[] { 0f, 0f, 1f },
        };

        var result = deduplicator.FilterBatch(vectors);

        result.Should().HaveCount(3);
    }

    [Fact]
    public void FilterBatch_WithDuplicates_ShouldFilterThem()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 0.99f);
        var vectors = new List<float[]>
        {
            new[] { 1f, 0f, 0f },
            new[] { 1f, 0f, 0f }, // Duplicate
            new[] { 0f, 1f, 0f },
        };

        var result = deduplicator.FilterBatch(vectors);

        result.Should().HaveCount(2);
    }

    [Fact]
    public void FilterBatch_EmptyList_ShouldReturnEmpty()
    {
        var deduplicator = new StreamDeduplicator();

        var result = deduplicator.FilterBatch(Enumerable.Empty<float[]>());

        result.Should().BeEmpty();
    }

    // --- FilterStreamAsync ---

    [Fact]
    public void FilterStreamAsync_WithNull_ShouldThrow()
    {
        var deduplicator = new StreamDeduplicator();

        var act = () => deduplicator.FilterStreamAsync(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task FilterStreamAsync_WithUniqueVectors_ShouldReturnAll()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 1.0f);
        var vectors = CreateAsyncEnumerable(
            new[] { 1f, 0f, 0f },
            new[] { 0f, 1f, 0f });

        var results = new List<float[]>();
        await foreach (var vector in deduplicator.FilterStreamAsync(vectors))
        {
            results.Add(vector);
        }

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task FilterStreamAsync_WithDuplicates_ShouldFilter()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 0.99f);
        var vectors = CreateAsyncEnumerable(
            new[] { 1f, 0f, 0f },
            new[] { 1f, 0f, 0f }, // Duplicate
            new[] { 0f, 1f, 0f });

        var results = new List<float[]>();
        await foreach (var vector in deduplicator.FilterStreamAsync(vectors))
        {
            results.Add(vector);
        }

        results.Should().HaveCount(2);
    }

    // --- LRU eviction ---

    [Fact]
    public void LruEviction_ShouldEvictLeastRecentlyUsed()
    {
        // Cache size 2: add A, B, C => A should be evicted
        var deduplicator = new StreamDeduplicator(similarityThreshold: 1.0f, maxCacheSize: 2);

        float[] vectorA = new[] { 1f, 0f, 0f };
        float[] vectorB = new[] { 0f, 1f, 0f };
        float[] vectorC = new[] { 0f, 0f, 1f };

        deduplicator.IsDuplicate(vectorA); // Cache: [A]
        deduplicator.IsDuplicate(vectorB); // Cache: [B, A]
        deduplicator.IsDuplicate(vectorC); // Cache: [C, B], A evicted

        // A should no longer be detected as duplicate
        bool aIsDuplicate = deduplicator.IsDuplicate(vectorA);
        aIsDuplicate.Should().BeFalse();
    }

    // --- Helper ---

    private static async IAsyncEnumerable<float[]> CreateAsyncEnumerable(params float[][] vectors)
    {
        foreach (var vector in vectors)
        {
            await Task.Yield();
            yield return vector;
        }
    }
}
