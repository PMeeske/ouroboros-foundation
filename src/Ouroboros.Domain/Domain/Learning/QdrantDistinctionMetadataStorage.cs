// <copyright file="QdrantDistinctionMetadataStorage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Ouroboros.Abstractions;
using Ouroboros.Core.Configuration;
using Ouroboros.Core.Learning;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Learning;

/// <summary>
/// Qdrant-based storage for distinction metadata with semantic search capabilities.
/// </summary>
public sealed class QdrantDistinctionMetadataStorage
{
    private readonly string _collectionName;
    private readonly QdrantClient _client;
    private readonly ILogger<QdrantDistinctionMetadataStorage> _logger;

    /// <summary>
    /// Initializes a new instance using the DI-provided client and collection registry.
    /// </summary>
    /// <param name="client">Shared Qdrant client from DI.</param>
    /// <param name="registry">Collection registry for role-based resolution.</param>
    /// <param name="logger">Logger instance.</param>
    public QdrantDistinctionMetadataStorage(
        QdrantClient client,
        IQdrantCollectionRegistry registry,
        ILogger<QdrantDistinctionMetadataStorage> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        ArgumentNullException.ThrowIfNull(registry);
        _collectionName = registry.GetCollectionName(QdrantCollectionRole.DistinctionStates);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantDistinctionMetadataStorage"/> class.
    /// </summary>
    /// <param name="connectionString">Qdrant connection string.</param>
    /// <param name="logger">Logger instance.</param>
    [Obsolete("Use the constructor accepting QdrantClient + IQdrantCollectionRegistry from DI.")]
    public QdrantDistinctionMetadataStorage(
        string connectionString,
        ILogger<QdrantDistinctionMetadataStorage> logger)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
        }

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collectionName = "distinction_states";

        // Parse connection string
        Uri uri = new Uri(connectionString);
        string host = uri.Host;
        int port = uri.Port > 0 ? uri.Port : 6334; // Fixed: use gRPC port
        bool useHttps = uri.Scheme == "https";

        _client = new QdrantClient(host, port, useHttps);
        _logger.LogInformation("Initialized Qdrant metadata storage: {Host}:{Port}", host, port);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantDistinctionMetadataStorage"/> class with existing client.
    /// </summary>
    /// <param name="client">Existing Qdrant client.</param>
    /// <param name="logger">Logger instance.</param>
    public QdrantDistinctionMetadataStorage(
        QdrantClient client,
        ILogger<QdrantDistinctionMetadataStorage> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collectionName = "distinction_states";
    }

    /// <summary>
    /// Stores distinction metadata with semantic embedding for retrieval.
    /// </summary>
    /// <param name="weights">The distinction weights containing metadata.</param>
    /// <param name="storagePath">The file system path where weights are stored.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result<Unit, string>> StoreMetadataAsync(
        DistinctionWeights weights,
        string storagePath,
        CancellationToken ct = default)
    {
        try
        {
            await EnsureCollectionExistsAsync(weights.Embedding.Length, ct);

            ulong pointId = BitConverter.ToUInt64(weights.Id.Value.ToByteArray(), 0);

            Dictionary<string, Value> payload = new Dictionary<string, Value>
            {
                ["id"] = weights.Id.ToString(),
                ["circumstance"] = weights.Circumstance,
                ["storage_path"] = storagePath,
                ["learned_at_stage"] = (int)weights.LearnedAtStage,
                ["fitness"] = weights.Fitness,
                ["is_dissolved"] = false,
                ["created_at"] = weights.CreatedAt.ToString("O"),
                ["dissolved_at"] = string.Empty
            };

            PointStruct point = new PointStruct
            {
                Id = new PointId { Num = pointId },
                Vectors = weights.Embedding,
                Payload = { payload }
            };

            await _client.UpsertAsync(_collectionName, new[] { point }, cancellationToken: ct);

            _logger.LogInformation("Stored metadata for distinction {Id}", weights.Id);
            return Result<Unit, string>.Success(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger.LogError(ex, "Qdrant RPC error storing metadata for distinction {Id}", weights.Id);
            return Result<Unit, string>.Failure($"Metadata storage RPC failed: {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store metadata for distinction {Id}", weights.Id);
            return Result<Unit, string>.Failure($"Metadata storage failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves similar distinctions by semantic search.
    /// </summary>
    /// <param name="query">The query text.</param>
    /// <param name="topK">Number of results to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing similar distinctions or an error.</returns>
    public async Task<Result<IReadOnlyList<DistinctionMetadata>, string>> SearchSimilarAsync(
        string query,
        int topK = 5,
        CancellationToken ct = default)
    {
        try
        {
            // For semantic search, we'd need an embedding model here
            // For now, we'll return an empty list as this requires additional infrastructure
            _logger.LogWarning("Semantic search not yet implemented - requires embedding model");
            return await Task.FromResult(Result<IReadOnlyList<DistinctionMetadata>, string>.Success(
                (IReadOnlyList<DistinctionMetadata>)Array.Empty<DistinctionMetadata>()));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger.LogError(ex, "Qdrant RPC error searching similar distinctions");
            return Result<IReadOnlyList<DistinctionMetadata>, string>.Failure($"Search RPC failed: {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search similar distinctions");
            return Result<IReadOnlyList<DistinctionMetadata>, string>.Failure($"Search failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Marks a distinction as dissolved.
    /// </summary>
    /// <param name="id">The distinction ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result<Unit, string>> MarkDissolvedAsync(
        DistinctionId id,
        CancellationToken ct = default)
    {
        try
        {
            // For simplicity, we'll delete and note this in logs
            // In a production system, you might want to update the payload
            ulong pointId = BitConverter.ToUInt64(id.Value.ToByteArray(), 0);

            await _client.DeleteAsync(
                _collectionName,
                new[] { new PointId { Num = pointId } },
                cancellationToken: ct);

            _logger.LogInformation("Marked distinction {Id} as dissolved (deleted from Qdrant)", id);
            return Result<Unit, string>.Success(Unit.Value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger.LogError(ex, "Qdrant RPC error marking distinction {Id} as dissolved", id);
            return Result<Unit, string>.Failure($"Update RPC failed: {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark distinction {Id} as dissolved", id);
            return Result<Unit, string>.Failure($"Update failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets metadata by ID.
    /// </summary>
    /// <param name="id">The distinction ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the metadata or an error.</returns>
    public async Task<Result<DistinctionMetadata, string>> GetByIdAsync(
        DistinctionId id,
        CancellationToken ct = default)
    {
        try
        {
            ulong pointId = BitConverter.ToUInt64(id.Value.ToByteArray(), 0);

            IReadOnlyList<RetrievedPoint> points = await _client.RetrieveAsync(
                _collectionName,
                new[] { new PointId { Num = pointId } },
                withPayload: true,
                cancellationToken: ct);

            if (points.Count == 0)
            {
                return Result<DistinctionMetadata, string>.Failure($"Distinction {id} not found");
            }

            RetrievedPoint point = points[0];
            DistinctionMetadata metadata = PayloadToMetadata(id, point.Payload);

            _logger.LogInformation("Retrieved metadata for distinction {Id}", id);
            return Result<DistinctionMetadata, string>.Success(metadata);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger.LogError(ex, "Qdrant RPC error getting metadata for distinction {Id}", id);
            return Result<DistinctionMetadata, string>.Failure($"Retrieval RPC failed: {ex.Status.Detail}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get metadata for distinction {Id}", id);
            return Result<DistinctionMetadata, string>.Failure($"Retrieval failed: {ex.Message}");
        }
    }

    private async Task EnsureCollectionExistsAsync(int vectorDimension, CancellationToken ct)
    {
        try
        {
            bool exists = await _client.CollectionExistsAsync(_collectionName, ct);

            if (!exists)
            {
                await _client.CreateCollectionAsync(
                    _collectionName,
                    new VectorParams { Size = (ulong)vectorDimension, Distance = Distance.Cosine },
                    cancellationToken: ct);

                _logger.LogInformation("Created collection {Collection} with dimension {Dimension}",
                    _collectionName, vectorDimension);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger.LogWarning(ex, "Qdrant RPC error ensuring collection exists (status: {Status})", ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ensure collection exists");
        }
    }

    private DistinctionMetadata PayloadToMetadata(DistinctionId id, IDictionary<string, Value> payload)
    {
        string circumstance = payload.TryGetValue("circumstance", out Value? c) ? c.StringValue : string.Empty;
        string storagePath = payload.TryGetValue("storage_path", out Value? sp) ? sp.StringValue : string.Empty;
        int learnedAtStage = payload.TryGetValue("learned_at_stage", out Value? stage)
            ? (int)stage.IntegerValue
            : 1; // Default to Distinction stage
        double fitness = payload.TryGetValue("fitness", out Value? f) ? f.DoubleValue : 0.0;
        bool isDissolved = payload.TryGetValue("is_dissolved", out Value? dissolved) && dissolved.BoolValue;
        DateTime createdAt = payload.TryGetValue("created_at", out Value? ca)
            ? DateTime.Parse(ca.StringValue)
            : DateTime.UtcNow;
        DateTime? dissolvedAt = payload.TryGetValue("dissolved_at", out Value? da) && !string.IsNullOrEmpty(da.StringValue)
            ? DateTime.Parse(da.StringValue)
            : (DateTime?)null;

        return new DistinctionMetadata(
            Id: id,
            Circumstance: circumstance,
            StoragePath: storagePath,
            LearnedAtStage: learnedAtStage,
            Fitness: fitness,
            IsDissolved: isDissolved,
            CreatedAt: createdAt,
            DissolvedAt: dissolvedAt);
    }
}