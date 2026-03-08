// <copyright file="InMemoryEmbodimentVectorStore.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// In-memory implementation of <see cref="IEmbodimentVectorStore"/> for testing
/// and environments without a vector database. Uses brute-force cosine similarity.
/// </summary>
public sealed class InMemoryEmbodimentVectorStore : IEmbodimentVectorStore
{
    private readonly List<(StoredPerceptionMeta Meta, float[] Vector)> _perceptions = [];
    private readonly List<(EmbodimentStateSnapshot Snapshot, float[] Vector)> _states = [];
    private readonly List<(AffordanceRecord Affordance, float[] Vector)> _affordances = [];
    private readonly object _lock = new();

    /// <inheritdoc/>
    public Task InitializeAsync(CancellationToken ct = default) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task StorePerceptionAsync(FusedPerception perception, float[] embedding, CancellationToken ct = default)
    {
        var meta = new StoredPerceptionMeta(
            perception.Id, perception.Timestamp, perception.DominantModality,
            perception.IntegratedUnderstanding, perception.Confidence);
        lock (_lock) _perceptions.Add((meta, embedding));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<RecalledPerception>> RecallPerceptionsAsync(
        float[] queryEmbedding, int limit = 5, SensorModality? modalityFilter = null, CancellationToken ct = default)
    {
        List<(StoredPerceptionMeta Meta, float[] Vector)> source;
        lock (_lock) source = _perceptions.ToList();

        var results = source
            .Where(p => modalityFilter == null || p.Meta.DominantModality == modalityFilter.Value)
            .Select(p => new RecalledPerception(p.Meta, CosineSimilarity(queryEmbedding, p.Vector)))
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToList();

        return Task.FromResult<IReadOnlyList<RecalledPerception>>(results);
    }

    /// <inheritdoc/>
    public Task StoreStateSnapshotAsync(EmbodimentStateSnapshot snapshot, float[] embedding, CancellationToken ct = default)
    {
        lock (_lock) _states.Add((snapshot, embedding));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<RecalledStateSnapshot>> RecallStatesAsync(
        float[] queryEmbedding, int limit = 5, CancellationToken ct = default)
    {
        List<(EmbodimentStateSnapshot Snapshot, float[] Vector)> source;
        lock (_lock) source = _states.ToList();

        var results = source
            .Select(s => new RecalledStateSnapshot(s.Snapshot, CosineSimilarity(queryEmbedding, s.Vector)))
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToList();

        return Task.FromResult<IReadOnlyList<RecalledStateSnapshot>>(results);
    }

    /// <inheritdoc/>
    public Task StoreAffordanceAsync(AffordanceRecord affordance, float[] embedding, CancellationToken ct = default)
    {
        lock (_lock) _affordances.Add((affordance, embedding));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ScoredAffordance>> FindAffordancesAsync(
        float[] queryEmbedding, int limit = 5, CancellationToken ct = default)
    {
        List<(AffordanceRecord Affordance, float[] Vector)> source;
        lock (_lock) source = _affordances.ToList();

        var results = source
            .Select(a => new ScoredAffordance(a.Affordance, CosineSimilarity(queryEmbedding, a.Vector)))
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToList();

        return Task.FromResult<IReadOnlyList<ScoredAffordance>>(results);
    }

    /// <inheritdoc/>
    public Task<EmbodimentVectorCounts> GetCountsAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(new EmbodimentVectorCounts(
                _perceptions.Count, _states.Count, _affordances.Count));
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0f;

        float dot = 0f, magA = 0f, magB = 0f;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        return magA == 0f || magB == 0f ? 0f : dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
    }
}
