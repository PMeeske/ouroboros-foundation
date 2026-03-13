// <copyright file="QdrantVectorStore.Admin.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Qdrant.Client.Grpc;
using LCVector = LangChain.Databases.Vector;
using LCDocument = LangChain.DocumentLoaders.Document;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Collection initialization, administration, and lifecycle operations.
/// </summary>
public sealed partial class QdrantVectorStore
{
    /// <summary>
    /// Adds vectors to the Qdrant store asynchronously.
    /// Creates the collection if it doesn't exist.
    /// </summary>
    public async Task AddAsync(IEnumerable<LCVector> vectors, CancellationToken cancellationToken = default)
    {
        List<LCVector> vectorList = vectors.ToList();
        if (!vectorList.Any())
        {
            _logger?.LogDebug("No vectors to add");
            return;
        }

        try
        {
            // Ensure collection exists
            await EnsureCollectionExistsAsync(vectorList[0], cancellationToken);

            // Convert vectors to Qdrant points
            List<PointStruct> points = vectorList.Select(v => new PointStruct
            {
                Id = new PointId { Uuid = Guid.NewGuid().ToString() },
                Vectors = v.Embedding?.ToArray() ?? Array.Empty<float>(),
                Payload =
                {
                    ["text"] = v.Text,
                    ["id"] = v.Id,
                }
            }).ToList();

            // Add metadata to payload
            foreach ((PointStruct? point, LCVector? vector) in points.Zip(vectorList))
            {
                if (vector.Metadata != null)
                {
                    foreach (KeyValuePair<string, object> kvp in vector.Metadata)
                    {
                        point.Payload[$"metadata_{kvp.Key}"] = kvp.Value?.ToString() ?? string.Empty;
                    }
                }
            }

            // Upsert points in batches
            const int batchSize = 100;
            for (int i = 0; i < points.Count; i += batchSize)
            {
                List<PointStruct> batch = points.Skip(i).Take(batchSize).ToList();
                await _client.UpsertAsync(_collectionName, batch, cancellationToken: cancellationToken);
                _logger?.LogDebug("Upserted batch of {Count} vectors to collection {Collection}", batch.Count, _collectionName);
            }

            _logger?.LogInformation("Added {Count} vectors to Qdrant collection {Collection}", vectorList.Count, _collectionName);
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to add vectors to Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to add vectors to Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Clears all vectors from the collection by deleting and recreating it.
    /// </summary>
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            bool collectionExists = await _client.CollectionExistsAsync(_collectionName, cancellationToken);
            if (collectionExists)
            {
                await _client.DeleteCollectionAsync(_collectionName, cancellationToken: cancellationToken);
                _logger?.LogInformation("Deleted Qdrant collection {Collection}", _collectionName);
            }
            else
            {
                _logger?.LogDebug("Collection {Collection} does not exist, nothing to clear", _collectionName);
            }
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to clear Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to clear Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteByIdAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            List<PointId> pointIds = ids.Select(id => Guid.TryParse(id, out Guid guid)
                ? new PointId { Uuid = guid.ToString() }
                : new PointId { Num = ulong.Parse(id) }).ToList();

            await _client.DeleteAsync(_collectionName, pointIds, cancellationToken: cancellationToken);

            _logger?.LogInformation("Deleted {Count} vectors by ID from collection {Collection}", pointIds.Count, _collectionName);
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to delete vectors by ID in Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to delete vectors by ID in Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteByFilterAsync(IDictionary<string, object> filter, CancellationToken cancellationToken = default)
    {
        try
        {
            Filter? qdrantFilter = BuildFilter(filter);
            if (qdrantFilter == null)
            {
                throw new ArgumentException("Filter cannot be empty for delete operation", nameof(filter));
            }

            await _client.DeleteAsync(_collectionName, qdrantFilter, cancellationToken: cancellationToken);

            _logger?.LogInformation("Deleted vectors by filter from collection {Collection}", _collectionName);
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to delete vectors by filter in Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to delete vectors by filter in Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<VectorStoreInfo> GetInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            bool collectionExists = await _client.CollectionExistsAsync(_collectionName, cancellationToken);
            if (!collectionExists)
            {
                return new VectorStoreInfo(_collectionName, 0, 0, "NotFound");
            }

            Qdrant.Client.Grpc.CollectionInfo info = await _client.GetCollectionInfoAsync(_collectionName, cancellationToken);
            ulong count = await _client.CountAsync(_collectionName, exact: true, cancellationToken: cancellationToken);

            ulong vectorDim = info.Config?.Params?.VectorsConfig?.Params?.Size ?? 0;
            string status = info.Status.ToString();

            Dictionary<string, object> additionalInfo = new Dictionary<string, object>
            {
                ["segments_count"] = info.SegmentsCount,
                ["points_count"] = info.PointsCount,
                ["indexed_vectors_count"] = info.IndexedVectorsCount,
                ["optimizer_status"] = info.OptimizerStatus?.Ok ?? false ? "OK" : "Optimizing"
            };

            _logger?.LogDebug("Collection info for {Collection}: {Count} vectors, {Dim}D, status={Status}",
                _collectionName, count, vectorDim, status);

            return new VectorStoreInfo(_collectionName, count, (int)vectorDim, status, additionalInfo);
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to get info for Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to get info for Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Ensures the collection exists with proper vector dimensions.
    /// </summary>
    private async Task EnsureCollectionExistsAsync(LCVector sampleVector, CancellationToken cancellationToken)
    {
        bool collectionExists = await _client.CollectionExistsAsync(_collectionName, cancellationToken);
        if (!collectionExists)
        {
            int vectorSize = sampleVector.Embedding?.Length ?? 0;
            if (vectorSize == 0)
            {
                throw new InvalidOperationException("Cannot create collection: sample vector has no embedding");
            }

            _logger?.LogInformation("Creating Qdrant collection {Collection} with vector size {Size}", _collectionName, vectorSize);

            await _client.CreateCollectionAsync(
                _collectionName,
                new VectorParams
                {
                    Size = (ulong)vectorSize,
                    Distance = Distance.Cosine
                },
                cancellationToken: cancellationToken);

            _logger?.LogInformation("Created Qdrant collection {Collection}", _collectionName);
        }
    }

    /// <summary>
    /// Disposes the Qdrant client if owned by this instance.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposeClient && _client != null)
        {
            _client.Dispose();
        }

        await Task.CompletedTask;
    }
}
