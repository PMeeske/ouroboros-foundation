// <copyright file="QdrantEmbodimentVectorStore.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Configuration;
using Ouroboros.Core.EmbodiedInteraction;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Qdrant-backed implementation of <see cref="IEmbodimentVectorStore"/>.
/// Stores perception, state, and affordance vectors in three separate collections
/// resolved via <see cref="IQdrantCollectionRegistry"/>.
/// Exchangeable: swap this for any other <see cref="IEmbodimentVectorStore"/> provider.
/// </summary>
public sealed class QdrantEmbodimentVectorStore : IEmbodimentVectorStore
{
    private readonly QdrantClient _client;
    private readonly string _perceptionsCollection;
    private readonly string _statesCollection;
    private readonly string _affordancesCollection;
    private readonly uint _vectorSize;
    private bool _initialized;

    /// <summary>
    /// Initializes a new Qdrant-backed embodiment vector store.
    /// </summary>
    /// <param name="client">Shared Qdrant gRPC client.</param>
    /// <param name="registry">Collection registry for role-based name resolution.</param>
    /// <param name="vectorSize">Embedding dimension (default 768 for nomic-embed-text).</param>
    public QdrantEmbodimentVectorStore(
        QdrantClient client,
        IQdrantCollectionRegistry registry,
        uint vectorSize = 768)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        ArgumentNullException.ThrowIfNull(registry);
        _perceptionsCollection = registry.GetCollectionName(QdrantCollectionRole.EmbodimentPerceptions);
        _statesCollection = registry.GetCollectionName(QdrantCollectionRole.EmbodimentStates);
        _affordancesCollection = registry.GetCollectionName(QdrantCollectionRole.EmbodimentAffordances);
        _vectorSize = vectorSize;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_initialized) return;

        await EnsureCollectionAsync(_perceptionsCollection, ct).ConfigureAwait(false);
        await EnsureCollectionAsync(_statesCollection, ct).ConfigureAwait(false);
        await EnsureCollectionAsync(_affordancesCollection, ct).ConfigureAwait(false);

        _initialized = true;
    }

    // ── Perceptions ──────────────────────────────────────────

    /// <inheritdoc/>
    public async Task StorePerceptionAsync(FusedPerception perception, float[] embedding, CancellationToken ct = default)
    {
        await InitializeAsync(ct).ConfigureAwait(false);

        var point = new PointStruct
        {
            Id = new PointId { Uuid = perception.Id.ToString() },
            Vectors = embedding,
            Payload =
            {
                ["timestamp"] = perception.Timestamp.ToString("O"),
                ["modality"] = perception.DominantModality.ToString(),
                ["understanding"] = perception.IntegratedUnderstanding,
                ["confidence"] = perception.Confidence.ToString("F3"),
                ["has_audio"] = perception.HasAudio.ToString(),
                ["has_visual"] = perception.HasVisual.ToString(),
            },
        };

        await _client.UpsertAsync(_perceptionsCollection, new[] { point }, cancellationToken: ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RecalledPerception>> RecallPerceptionsAsync(
        float[] queryEmbedding, int limit = 5, SensorModality? modalityFilter = null, CancellationToken ct = default)
    {
        await InitializeAsync(ct).ConfigureAwait(false);

        Filter? filter = null;
        if (modalityFilter.HasValue)
        {
            filter = new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "modality",
                            Match = new Match { Keyword = modalityFilter.Value.ToString() },
                        },
                    },
                },
            };
        }

        var results = await _client.SearchAsync(
            _perceptionsCollection, queryEmbedding, filter: filter,
            limit: (ulong)limit, cancellationToken: ct).ConfigureAwait(false);

        return results.Select(r =>
        {
            var meta = new StoredPerceptionMeta(
                Guid.TryParse(r.Id.Uuid, out var id) ? id : Guid.Empty,
                DateTime.TryParse(GetPayload(r, "timestamp"), out var ts) ? ts : DateTime.MinValue,
                Enum.TryParse<SensorModality>(GetPayload(r, "modality"), out var mod) ? mod : SensorModality.Text,
                GetPayload(r, "understanding"),
                double.TryParse(GetPayload(r, "confidence"), out var c) ? c : 0);
            return new RecalledPerception(meta, r.Score);
        }).ToList();
    }

    // ── State Snapshots ──────────────────────────────────────

    /// <inheritdoc/>
    public async Task StoreStateSnapshotAsync(EmbodimentStateSnapshot snapshot, float[] embedding, CancellationToken ct = default)
    {
        await InitializeAsync(ct).ConfigureAwait(false);

        var point = new PointStruct
        {
            Id = new PointId { Uuid = snapshot.Id.ToString() },
            Vectors = embedding,
            Payload =
            {
                ["timestamp"] = snapshot.Timestamp.ToString("O"),
                ["state"] = snapshot.State.ToString(),
                ["description"] = snapshot.Description,
                ["energy"] = snapshot.EnergyLevel.ToString("F3"),
                ["active_sensors"] = string.Join(",", snapshot.ActiveSensors),
                ["attention_target"] = snapshot.AttentionTarget ?? string.Empty,
            },
        };

        await _client.UpsertAsync(_statesCollection, new[] { point }, cancellationToken: ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<RecalledStateSnapshot>> RecallStatesAsync(
        float[] queryEmbedding, int limit = 5, CancellationToken ct = default)
    {
        await InitializeAsync(ct).ConfigureAwait(false);

        var results = await _client.SearchAsync(
            _statesCollection, queryEmbedding, limit: (ulong)limit, cancellationToken: ct).ConfigureAwait(false);

        return results.Select(r =>
        {
            var sensorsStr = GetPayload(r, "active_sensors");
            var sensors = string.IsNullOrEmpty(sensorsStr)
                ? new HashSet<SensorModality>()
                : new HashSet<SensorModality>(
                    sensorsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => Enum.TryParse<SensorModality>(s, out var m) ? m : SensorModality.Text));

            var snapshot = new EmbodimentStateSnapshot(
                Guid.TryParse(r.Id.Uuid, out var id) ? id : Guid.Empty,
                DateTime.TryParse(GetPayload(r, "timestamp"), out var ts) ? ts : DateTime.MinValue,
                Enum.TryParse<EmbodimentState>(GetPayload(r, "state"), out var st) ? st : EmbodimentState.Dormant,
                GetPayload(r, "description"),
                double.TryParse(GetPayload(r, "energy"), out var e) ? e : 0.5,
                sensors,
                GetPayload(r, "attention_target") is { Length: > 0 } att ? att : null);
            return new RecalledStateSnapshot(snapshot, r.Score);
        }).ToList();
    }

    // ── Affordances ──────────────────────────────────────────

    /// <inheritdoc/>
    public async Task StoreAffordanceAsync(AffordanceRecord affordance, float[] embedding, CancellationToken ct = default)
    {
        await InitializeAsync(ct).ConfigureAwait(false);

        var point = new PointStruct
        {
            Id = new PointId { Uuid = affordance.Id.ToString() },
            Vectors = embedding,
            Payload =
            {
                ["description"] = affordance.Description,
                ["type"] = affordance.Type.ToString(),
                ["constraints"] = affordance.Constraints ?? string.Empty,
                ["created_at"] = affordance.CreatedAt.ToString("O"),
            },
        };

        await _client.UpsertAsync(_affordancesCollection, new[] { point }, cancellationToken: ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScoredAffordance>> FindAffordancesAsync(
        float[] queryEmbedding, int limit = 5, CancellationToken ct = default)
    {
        await InitializeAsync(ct).ConfigureAwait(false);

        var results = await _client.SearchAsync(
            _affordancesCollection, queryEmbedding, limit: (ulong)limit, cancellationToken: ct).ConfigureAwait(false);

        return results.Select(r =>
        {
            var aff = new AffordanceRecord(
                Guid.TryParse(r.Id.Uuid, out var id) ? id : Guid.Empty,
                GetPayload(r, "description"),
                Enum.TryParse<AffordanceType>(GetPayload(r, "type"), out var t) ? t : AffordanceType.Custom,
                GetPayload(r, "constraints") is { Length: > 0 } c ? c : null,
                DateTime.TryParse(GetPayload(r, "created_at"), out var ts) ? ts : DateTime.MinValue);
            return new ScoredAffordance(aff, r.Score);
        }).ToList();
    }

    // ── Counts ───────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<EmbodimentVectorCounts> GetCountsAsync(CancellationToken ct = default)
    {
        await InitializeAsync(ct).ConfigureAwait(false);

        var pInfo = await _client.GetCollectionInfoAsync(_perceptionsCollection, ct).ConfigureAwait(false);
        var sInfo = await _client.GetCollectionInfoAsync(_statesCollection, ct).ConfigureAwait(false);
        var aInfo = await _client.GetCollectionInfoAsync(_affordancesCollection, ct).ConfigureAwait(false);

        return new EmbodimentVectorCounts(
            (long)pInfo.PointsCount,
            (long)sInfo.PointsCount,
            (long)aInfo.PointsCount);
    }

    // ── Helpers ──────────────────────────────────────────────

    private async Task EnsureCollectionAsync(string name, CancellationToken ct)
    {
        if (await _client.CollectionExistsAsync(name, ct).ConfigureAwait(false))
            return;

        await _client.CreateCollectionAsync(
            name,
            new VectorParams { Size = _vectorSize, Distance = Distance.Cosine },
            cancellationToken: ct).ConfigureAwait(false);
    }

    private static string GetPayload(ScoredPoint point, string key)
        => point.Payload.TryGetValue(key, out var val) ? val.StringValue : string.Empty;

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
