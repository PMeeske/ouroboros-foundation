// <copyright file="QdrantVectorStore.Search.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Qdrant.Client.Grpc;
using LCVector = LangChain.Databases.Vector;
using LCDocument = LangChain.DocumentLoaders.Document;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Search and query operations for the Qdrant vector store.
/// </summary>
public sealed partial class QdrantVectorStore
{
    /// <summary>
    /// Performs similarity search and returns the most similar documents.
    /// </summary>
    public async Task<IReadOnlyCollection<LCDocument>> GetSimilarDocumentsAsync(
        float[] embedding,
        int amount = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if collection exists
            bool collectionExists = await _client.CollectionExistsAsync(_collectionName, cancellationToken);
            if (!collectionExists)
            {
                _logger?.LogDebug("Collection {Collection} does not exist, returning empty results", _collectionName);
                return Array.Empty<LCDocument>();
            }

            // Perform similarity search
            IReadOnlyList<ScoredPoint> searchResult = await _client.SearchAsync(
                _collectionName,
                embedding,
                limit: (ulong)amount,
                cancellationToken: cancellationToken);

            // Convert results to documents
            List<LCDocument> documents = searchResult.Select(scored =>
            {
                string text = scored.Payload.TryGetValue("text", out Value? textValue)
                    ? textValue.StringValue
                    : string.Empty;

                Dictionary<string, object> metadata = new Dictionary<string, object>();
                foreach (KeyValuePair<string, Value> kvp in scored.Payload)
                {
                    if (kvp.Key.StartsWith("metadata_"))
                    {
                        string key = kvp.Key["metadata_".Length..];
                        metadata[key] = kvp.Value.StringValue;
                    }
                    else if (kvp.Key != "text")
                    {
                        metadata[kvp.Key] = kvp.Value.StringValue;
                    }
                }

                metadata["score"] = scored.Score;

                return new LCDocument(text, metadata);
            }).ToList();

            _logger?.LogDebug("Found {Count} similar documents in collection {Collection}", documents.Count, _collectionName);
            return documents.AsReadOnly();
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to search vectors in Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to search vectors in Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Gets all vectors currently stored in the collection using scroll pagination.
    /// </summary>
    public async Task<IEnumerable<LCVector>> GetAllAsync()
    {
        List<LCVector> results = new List<LCVector>();
        PointId? offset = null;

        try
        {
            bool collectionExists = await _client.CollectionExistsAsync(_collectionName);
            if (!collectionExists)
            {
                return results;
            }

            do
            {
                ScrollResponse scrollResult = await _client.ScrollAsync(
                    _collectionName,
                    limit: 100,
                    offset: offset,
                    payloadSelector: new WithPayloadSelector { Enable = true },
                    vectorsSelector: new WithVectorsSelector { Enable = true });

                foreach (RetrievedPoint? point in scrollResult.Result)
                {
                    string text = point.Payload.TryGetValue("text", out Value? textValue)
                        ? textValue.StringValue
                        : string.Empty;
                    string id = point.Payload.TryGetValue("id", out Value? idValue)
                        ? idValue.StringValue
                        : point.Id.Uuid ?? point.Id.Num.ToString();

                    float[]? embedding = null;
#pragma warning disable CS0612 // VectorOutput.Data is obsolete but still functional
                    if (point.Vectors?.Vector?.Data != null)
                    {
                        embedding = point.Vectors.Vector.Data.ToArray();
                    }
#pragma warning restore CS0612

                    results.Add(new LCVector
                    {
                        Id = id,
                        Text = text,
                        Embedding = embedding
                    });
                }

                offset = scrollResult.Result.Count > 0 ? scrollResult.Result[^1].Id : null;
            }
            while (offset != null);

            _logger?.LogDebug("Retrieved {Count} vectors from collection {Collection}", results.Count, _collectionName);
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to get all vectors from Qdrant collection {Collection}", _collectionName);
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to get all vectors from Qdrant collection {Collection}", _collectionName);
        }

        return results;
    }

    /// <summary>
    /// Gets all vectors synchronously. Prefer <see cref="GetAllAsync"/> for async callers.
    /// </summary>
    public IEnumerable<LCVector> GetAll()
    {
        // Use Task.Run to avoid deadlocks from synchronization context capture
        // Intentional: sync wrapper for non-async callers
        return Task.Run(() => GetAllAsync()).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<LCDocument>> SearchWithFilterAsync(
        float[] embedding,
        IDictionary<string, object>? filter = null,
        int amount = 5,
        float? scoreThreshold = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool collectionExists = await _client.CollectionExistsAsync(_collectionName, cancellationToken);
            if (!collectionExists)
            {
                return Array.Empty<LCDocument>();
            }

            Filter? qdrantFilter = BuildFilter(filter);

            IReadOnlyList<ScoredPoint> searchResult = await _client.SearchAsync(
                _collectionName,
                embedding,
                filter: qdrantFilter,
                limit: (ulong)amount,
                scoreThreshold: scoreThreshold,
                cancellationToken: cancellationToken);

            return ConvertToDocuments(searchResult);
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to search with filter in Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to search with filter in Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ulong> CountAsync(IDictionary<string, object>? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            bool collectionExists = await _client.CollectionExistsAsync(_collectionName, cancellationToken);
            if (!collectionExists)
            {
                return 0;
            }

            Filter? qdrantFilter = BuildFilter(filter);
            ulong count = await _client.CountAsync(_collectionName, filter: qdrantFilter, exact: true, cancellationToken: cancellationToken);

            _logger?.LogDebug("Count in collection {Collection}: {Count}", _collectionName, count);
            return count;
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to count vectors in Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to count vectors in Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ScrollResult> ScrollAsync(
        int limit = 10,
        string? offset = null,
        IDictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool collectionExists = await _client.CollectionExistsAsync(_collectionName, cancellationToken);
            if (!collectionExists)
            {
                return new ScrollResult(Array.Empty<LCDocument>(), null);
            }

            PointId? pointOffset = null;
            if (!string.IsNullOrEmpty(offset))
            {
                pointOffset = Guid.TryParse(offset, out Guid guid)
                    ? new PointId { Uuid = guid.ToString() }
                    : new PointId { Num = ulong.Parse(offset) };
            }

            Filter? qdrantFilter = BuildFilter(filter);

            ScrollResponse scrollResult = await _client.ScrollAsync(
                _collectionName,
                filter: qdrantFilter,
                limit: (uint)limit,
                offset: pointOffset,
                payloadSelector: new WithPayloadSelector { Enable = true },
                cancellationToken: cancellationToken);

            List<LCDocument> documents = scrollResult.Result.Select(point =>
            {
                string text = point.Payload.TryGetValue("text", out Value? textValue)
                    ? textValue.StringValue
                    : string.Empty;

                Dictionary<string, object> metadata = ExtractMetadata(point.Payload);
                metadata["id"] = point.Id.Uuid ?? point.Id.Num.ToString();

                return new LCDocument(text, metadata);
            }).ToList();

            string? nextOffset = scrollResult.Result.Count > 0
                ? scrollResult.Result[^1].Id.Uuid ?? scrollResult.Result[^1].Id.Num.ToString()
                : null;

            _logger?.LogDebug("Scrolled {Count} documents from collection {Collection}", documents.Count, _collectionName);
            return new ScrollResult(documents, nextOffset);
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to scroll in Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to scroll in Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IReadOnlyCollection<LCDocument>>> BatchSearchAsync(
        IReadOnlyList<float[]> embeddings,
        int amount = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool collectionExists = await _client.CollectionExistsAsync(_collectionName, cancellationToken);
            if (!collectionExists)
            {
                return embeddings.Select(_ => (IReadOnlyCollection<LCDocument>)Array.Empty<LCDocument>()).ToList();
            }

            List<SearchPoints> searches = embeddings.Select(e => new SearchPoints
            {
                CollectionName = _collectionName,
                Vector = { e },
                Limit = (ulong)amount,
                WithPayload = new WithPayloadSelector { Enable = true }
            }).ToList();

            IReadOnlyList<BatchResult> batchResults = await _client.SearchBatchAsync(_collectionName, searches, cancellationToken: cancellationToken);

            List<IReadOnlyCollection<LCDocument>> results = batchResults.Select(batch =>
                (IReadOnlyCollection<LCDocument>)ConvertToDocuments(batch.Result)).ToList();

            _logger?.LogDebug("Batch search completed: {Queries} queries, {TotalResults} total results",
                embeddings.Count, results.Sum(r => r.Count));
            return results;
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to batch search in Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to batch search in Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<LCDocument>> RecommendAsync(
        IReadOnlyList<string> positiveIds,
        IReadOnlyList<string>? negativeIds = null,
        int amount = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            bool collectionExists = await _client.CollectionExistsAsync(_collectionName, cancellationToken);
            if (!collectionExists)
            {
                return Array.Empty<LCDocument>();
            }

            List<PointId> positive = positiveIds.Select(id => Guid.TryParse(id, out Guid guid)
                ? new PointId { Uuid = guid.ToString() }
                : new PointId { Num = ulong.Parse(id) }).ToList();

            List<PointId>? negative = negativeIds?.Select(id => Guid.TryParse(id, out Guid guid)
                ? new PointId { Uuid = guid.ToString() }
                : new PointId { Num = ulong.Parse(id) }).ToList();

            IReadOnlyList<ScoredPoint> results = await _client.RecommendAsync(
                _collectionName,
                positive,
                negative,
                limit: (ulong)amount,
                payloadSelector: new WithPayloadSelector { Enable = true },
                cancellationToken: cancellationToken);

            _logger?.LogDebug("Recommend returned {Count} results from collection {Collection}", results.Count, _collectionName);
            return ConvertToDocuments(results);
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogError(ex, "Failed to recommend in Qdrant collection {Collection}", _collectionName);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Failed to recommend in Qdrant collection {Collection}", _collectionName);
            throw;
        }
    }
}
