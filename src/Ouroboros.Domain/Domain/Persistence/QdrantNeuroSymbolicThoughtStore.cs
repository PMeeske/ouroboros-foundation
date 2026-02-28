// <copyright file="QdrantNeuroSymbolicThoughtStore.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Text.Json;
using Google.Protobuf.Collections;
using Ouroboros.Core.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Qdrant-backed neuro-symbolic thought store.
/// Combines vector embeddings (neural) with typed relations (symbolic) for rich thought persistence.
/// Enables semantic search, causal reasoning, and symbolic graph queries.
/// </summary>
public sealed partial class QdrantNeuroSymbolicThoughtStore : IThoughtStore, IAsyncDisposable
{
    private readonly QdrantNeuroSymbolicConfig _config;
    private readonly QdrantClient _client;
    private readonly Func<string, Task<float[]>>? _embeddingFunc;
    private bool _initialized;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance using the DI-provided client and collection registry.
    /// </summary>
    /// <param name="client">Shared Qdrant client from DI.</param>
    /// <param name="registry">Collection registry for role-based resolution.</param>
    /// <param name="settings">Qdrant settings for vector size.</param>
    /// <param name="embeddingFunc">Function to generate embeddings for thoughts.</param>
    public QdrantNeuroSymbolicThoughtStore(
        QdrantClient client,
        IQdrantCollectionRegistry registry,
        QdrantSettings settings,
        Func<string, Task<float[]>>? embeddingFunc = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(settings);
        _embeddingFunc = embeddingFunc;
        _config = new QdrantNeuroSymbolicConfig(
            Endpoint: settings.GrpcEndpoint,
            ThoughtsCollection: registry.GetCollectionName(QdrantCollectionRole.NeuroThoughts),
            RelationsCollection: registry.GetCollectionName(QdrantCollectionRole.ThoughtRelations),
            ResultsCollection: registry.GetCollectionName(QdrantCollectionRole.ThoughtResults),
            VectorSize: settings.DefaultVectorSize);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantNeuroSymbolicThoughtStore"/> class.
    /// </summary>
    /// <param name="config">Configuration for the store.</param>
    /// <param name="embeddingFunc">Function to generate embeddings for thoughts.</param>
    [Obsolete("Use the constructor accepting QdrantClient + IQdrantCollectionRegistry from DI.")]
    public QdrantNeuroSymbolicThoughtStore(
        QdrantNeuroSymbolicConfig config,
        Func<string, Task<float[]>>? embeddingFunc = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _embeddingFunc = embeddingFunc;

        Uri uri = new Uri(config.Endpoint);
        _client = new QdrantClient(uri.Host, uri.Port > 0 ? uri.Port : 6334, uri.Scheme == "https");
    }

    /// <summary>
    /// Gets whether semantic search is available.
    /// </summary>
    public bool SupportsSemanticSearch => _embeddingFunc != null;

    /// <summary>
    /// Initializes the Qdrant collections.
    /// </summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_initialized) return;

        try
        {
            // Create thoughts collection
            if (!await _client.CollectionExistsAsync(_config.ThoughtsCollection, ct))
            {
                await _client.CreateCollectionAsync(
                    _config.ThoughtsCollection,
                    new VectorParams { Size = (ulong)_config.VectorSize, Distance = Distance.Cosine },
                    cancellationToken: ct);
            }

            // Create relations collection
            if (!await _client.CollectionExistsAsync(_config.RelationsCollection, ct))
            {
                await _client.CreateCollectionAsync(
                    _config.RelationsCollection,
                    new VectorParams { Size = (ulong)_config.VectorSize, Distance = Distance.Cosine },
                    cancellationToken: ct);
            }

            // Create results collection
            if (!await _client.CollectionExistsAsync(_config.ResultsCollection, ct))
            {
                await _client.CreateCollectionAsync(
                    _config.ResultsCollection,
                    new VectorParams { Size = (ulong)_config.VectorSize, Distance = Distance.Cosine },
                    cancellationToken: ct);
            }

            _initialized = true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            throw new InvalidOperationException($"Qdrant RPC error initializing neuro-symbolic collections: {ex.Status.Detail}", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to initialize neuro-symbolic collections: {ex.Message}", ex);
        }
    }

    #region IThoughtStore Implementation

    /// <inheritdoc/>
    public async Task SaveThoughtAsync(string sessionId, PersistedThought thought, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct);

        float[] embedding = await GenerateEmbeddingAsync(thought.Content, ct);
        PointStruct point = CreateThoughtPoint(sessionId, thought, embedding);

        await _client.UpsertAsync(_config.ThoughtsCollection, new[] { point }, cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async Task SaveThoughtsAsync(string sessionId, IEnumerable<PersistedThought> thoughts, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct);

        List<PointStruct> points = new List<PointStruct>();
        foreach (PersistedThought thought in thoughts)
        {
            float[] embedding = await GenerateEmbeddingAsync(thought.Content, ct);
            points.Add(CreateThoughtPoint(sessionId, thought, embedding));
        }

        if (points.Count > 0)
        {
            // Batch upsert
            const int batchSize = 100;
            for (int i = 0; i < points.Count; i += batchSize)
            {
                List<PointStruct> batch = points.Skip(i).Take(batchSize).ToList();
                await _client.UpsertAsync(_config.ThoughtsCollection, batch, cancellationToken: ct);
            }
        }
    }

    // Query methods are in QdrantNeuroSymbolicThoughtStore.Query.cs

    /// <inheritdoc/>
    public async Task ClearSessionAsync(string sessionId, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct);

        Filter filter = CreateSessionFilter(sessionId);

        // Delete from all collections
        await _client.DeleteAsync(_config.ThoughtsCollection, filter, cancellationToken: ct);
        await _client.DeleteAsync(_config.RelationsCollection, filter, cancellationToken: ct);
        await _client.DeleteAsync(_config.ResultsCollection, filter, cancellationToken: ct);
    }

    // Statistics and listing methods are in QdrantNeuroSymbolicThoughtStore.Query.cs

    #endregion

    #region Neuro-Symbolic Extensions

    /// <summary>
    /// Saves a thought with automatic relation inference to recent thoughts.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="thought">The thought to save.</param>
    /// <param name="autoInferRelations">Whether to automatically infer relations to recent thoughts.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task SaveWithRelationsAsync(
        string sessionId,
        PersistedThought thought,
        bool autoInferRelations = true,
        CancellationToken ct = default)
    {
        await SaveThoughtAsync(sessionId, thought, ct);

        if (autoInferRelations && _embeddingFunc != null)
        {
            // Find semantically similar recent thoughts
            IReadOnlyList<PersistedThought> recent = await GetRecentThoughtsAsync(sessionId, 10, ct);
            float[] thoughtEmbedding = await _embeddingFunc(thought.Content);

            foreach (PersistedThought? recentThought in recent.Where(r => r.Id != thought.Id))
            {
                float[] recentEmbedding = await _embeddingFunc(recentThought.Content);
                double similarity = CosineSimilarity(thoughtEmbedding, recentEmbedding);

                if (similarity > 0.7) // High similarity threshold
                {
                    // Infer relation type based on thought types and content
                    string relationType = InferRelationType(thought, recentThought);
                    ThoughtRelation relation = new ThoughtRelation(
                        Guid.NewGuid(),
                        recentThought.Id,
                        thought.Id,
                        relationType,
                        similarity,
                        DateTime.UtcNow);

                    await SaveRelationAsync(sessionId, relation, ct);
                }
            }
        }
    }

    /// <summary>
    /// Saves a relation between two thoughts.
    /// </summary>
    public async Task SaveRelationAsync(string sessionId, ThoughtRelation relation, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct);

        string text = $"{relation.RelationType}: {relation.SourceThoughtId} -> {relation.TargetThoughtId}";
        float[] embedding = await GenerateEmbeddingAsync(text, ct);

        Dictionary<string, Value> payload = new Dictionary<string, Value>
        {
            ["id"] = relation.Id.ToString(),
            ["session_id"] = sessionId,
            ["source_thought_id"] = relation.SourceThoughtId.ToString(),
            ["target_thought_id"] = relation.TargetThoughtId.ToString(),
            ["relation_type"] = relation.RelationType,
            ["strength"] = relation.Strength,
            ["created_at"] = relation.CreatedAt.ToString("O")
        };

        if (relation.Metadata != null)
        {
            payload["metadata_json"] = JsonSerializer.Serialize(relation.Metadata);
        }

        PointStruct point = new PointStruct
        {
            Id = new PointId { Uuid = relation.Id.ToString() },
            Vectors = embedding,
            Payload = { payload }
        };

        await _client.UpsertAsync(_config.RelationsCollection, new[] { point }, cancellationToken: ct);
    }

    /// <summary>
    /// Saves the result/outcome of a thought.
    /// </summary>
    public async Task SaveResultAsync(string sessionId, ThoughtResult result, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct);

        float[] embedding = await GenerateEmbeddingAsync(result.Content, ct);

        Dictionary<string, Value> payload = new Dictionary<string, Value>
        {
            ["id"] = result.Id.ToString(),
            ["session_id"] = sessionId,
            ["thought_id"] = result.ThoughtId.ToString(),
            ["result_type"] = result.ResultType,
            ["content"] = result.Content,
            ["success"] = result.Success,
            ["confidence"] = result.Confidence,
            ["created_at"] = result.CreatedAt.ToString("O")
        };

        if (result.ExecutionTime.HasValue)
        {
            payload["execution_time_ms"] = result.ExecutionTime.Value.TotalMilliseconds;
        }

        if (result.Metadata != null)
        {
            payload["metadata_json"] = JsonSerializer.Serialize(result.Metadata);
        }

        PointStruct point = new PointStruct
        {
            Id = new PointId { Uuid = result.Id.ToString() },
            Vectors = embedding,
            Payload = { payload }
        };

        await _client.UpsertAsync(_config.ResultsCollection, new[] { point }, cancellationToken: ct);

        // Create relation from thought to result
        ThoughtRelation relation = new ThoughtRelation(
            Guid.NewGuid(),
            result.ThoughtId,
            result.Id,
            result.Success ? ThoughtRelation.Types.LeadsTo : ThoughtRelation.Types.Triggers,
            result.Confidence,
            DateTime.UtcNow);

        await SaveRelationAsync(sessionId, relation, ct);
    }

    // Query extension methods are in QdrantNeuroSymbolicThoughtStore.Query.cs

    #endregion

    // Private helpers, serialization, chain traversal, and disposal are in
    // QdrantNeuroSymbolicThoughtStore.Helpers.cs
}
