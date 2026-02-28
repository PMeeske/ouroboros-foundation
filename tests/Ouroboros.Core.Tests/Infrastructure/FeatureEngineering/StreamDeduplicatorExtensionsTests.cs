// <copyright file="StreamDeduplicatorExtensionsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Infrastructure.FeatureEngineering;

namespace Ouroboros.Tests.Infrastructure.FeatureEngineering;

[Trait("Category", "Unit")]
public class StreamDeduplicatorExtensionsTests
{
    // --- IAsyncEnumerable Deduplicate ---

    [Fact]
    public void Deduplicate_AsyncEnumerable_NullVectors_ShouldThrow()
    {
        IAsyncEnumerable<float[]>? nullVectors = null;
        var deduplicator = new StreamDeduplicator();

        var act = () => nullVectors!.Deduplicate(deduplicator);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deduplicate_AsyncEnumerable_NullDeduplicator_ShouldThrow()
    {
        var vectors = CreateAsyncEnumerable(new[] { 1f, 0f });

        var act = () => vectors.Deduplicate(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Deduplicate_AsyncEnumerable_ShouldFilterDuplicates()
    {
        var deduplicator = new StreamDeduplicator(similarityThreshold: 0.99f);
        var vectors = CreateAsyncEnumerable(
            new[] { 1f, 0f, 0f },
            new[] { 1f, 0f, 0f }, // Duplicate
            new[] { 0f, 1f, 0f });

        var results = new List<float[]>();
        await foreach (var vector in vectors.Deduplicate(deduplicator))
        {
            results.Add(vector);
        }

        results.Should().HaveCount(2);
    }

    // --- IEnumerable Deduplicate ---

    [Fact]
    public void Deduplicate_SyncEnumerable_NullVectors_ShouldThrow()
    {
        IEnumerable<float[]>? nullVectors = null;

        var act = () => nullVectors!.Deduplicate();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deduplicate_SyncEnumerable_ShouldFilterDuplicates()
    {
        var vectors = new List<float[]>
        {
            new[] { 1f, 0f, 0f },
            new[] { 1f, 0f, 0f }, // Duplicate
            new[] { 0f, 1f, 0f },
        };

        var result = vectors.Deduplicate(similarityThreshold: 0.99f);

        result.Should().HaveCount(2);
    }

    [Fact]
    public void Deduplicate_SyncEnumerable_WithCustomThreshold_ShouldUseThreshold()
    {
        // With threshold 0, nothing is considered duplicate (similarity must be >= 0, and it always is)
        // With threshold 1.0, only exact identical vectors are duplicates
        var vectors = new List<float[]>
        {
            new[] { 1f, 0f, 0f },
            new[] { 0.9999f, 0.01f, 0f }, // Very similar but not identical
        };

        var resultStrict = vectors.Deduplicate(similarityThreshold: 1.0f);
        resultStrict.Should().HaveCount(2); // Not exact duplicates

        var resultLoose = vectors.Deduplicate(similarityThreshold: 0.5f);
        resultLoose.Should().HaveCount(1); // Very similar, counted as duplicate
    }

    [Fact]
    public void Deduplicate_SyncEnumerable_EmptyList_ShouldReturnEmpty()
    {
        var vectors = new List<float[]>();

        var result = vectors.Deduplicate();

        result.Should().BeEmpty();
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
