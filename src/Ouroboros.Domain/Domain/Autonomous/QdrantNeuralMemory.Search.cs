// <copyright file="QdrantNeuralMemory.Search.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Google.Protobuf.Collections;
using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Search and statistics methods for QdrantNeuralMemory.
/// </summary>
public sealed partial class QdrantNeuralMemory
{
    /// <summary>
    /// Searches for similar neuron messages.
    /// </summary>
    public async Task<IReadOnlyList<NeuronMessage>> SearchSimilarMessagesAsync(
        float[] queryVector, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            bool exists = await _client.CollectionExistsAsync(_neuronMessagesCollection, ct);
            if (!exists) return Array.Empty<NeuronMessage>();

            IReadOnlyList<ScoredPoint> results = await _client.SearchAsync(
                _neuronMessagesCollection,
                queryVector,
                limit: (ulong)limit,
                payloadSelector: new WithPayloadSelector { Enable = true },
                cancellationToken: ct);

            return results.Select(r =>
            {
                MapField<string, Value> p = r.Payload;
                return new NeuronMessage
                {
                    Id = Guid.TryParse(r.Id.Uuid, out Guid id) ? id : Guid.NewGuid(),
                    SourceNeuron = p.TryGetValue("source_neuron", out Value? sn) ? sn.StringValue : "",
                    TargetNeuron = p.TryGetValue("target_neuron", out Value? tn) ? tn.StringValue : null,
                    Topic = p.TryGetValue("topic", out Value? t) ? t.StringValue : "",
                    Payload = p.TryGetValue("content", out Value? c) ? c.StringValue : "",
                    Priority = p.TryGetValue("priority", out Value? pr) && Enum.TryParse<IntentionPriority>(pr.IntegerValue.ToString(), out IntentionPriority pv) ? pv : IntentionPriority.Normal,
                };
            }).ToList();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Qdrant RPC search error: {ex.Status.Detail}");
            return Array.Empty<NeuronMessage>();
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search failed: {ex.Message}");
            return Array.Empty<NeuronMessage>();
        }
    }

    /// <summary>
    /// Searches for similar memories.
    /// </summary>
    public async Task<IReadOnlyList<string>> SearchMemoriesAsync(
        float[] queryVector, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            bool exists = await _client.CollectionExistsAsync(_memoriesCollection, ct);
            if (!exists) return Array.Empty<string>();

            IReadOnlyList<ScoredPoint> results = await _client.SearchAsync(
                _memoriesCollection,
                queryVector,
                limit: (ulong)limit,
                payloadSelector: new WithPayloadSelector { Enable = true },
                cancellationToken: ct);

            return results.Select(r =>
                r.Payload.TryGetValue("content", out Value? c) ? c.StringValue : "").ToList();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Qdrant RPC search error: {ex.Status.Detail}");
            return Array.Empty<string>();
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search failed: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Searches for similar intentions.
    /// </summary>
    public async Task<IReadOnlyList<Intention>> SearchIntentionsAsync(
        float[] queryVector, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            bool exists = await _client.CollectionExistsAsync(_intentionsCollection, ct);
            if (!exists) return Array.Empty<Intention>();

            IReadOnlyList<ScoredPoint> results = await _client.SearchAsync(
                _intentionsCollection,
                queryVector,
                limit: (ulong)limit,
                payloadSelector: new WithPayloadSelector { Enable = true },
                cancellationToken: ct);

            return results.Select(r =>
            {
                MapField<string, Value> p = r.Payload;
                return new Intention
                {
                    Id = Guid.TryParse(r.Id.Uuid, out Guid id) ? id : Guid.NewGuid(),
                    Title = p.TryGetValue("title", out Value? ti) ? ti.StringValue : "",
                    Description = p.TryGetValue("description", out Value? d) ? d.StringValue : "",
                    Rationale = p.TryGetValue("rationale", out Value? ra) ? ra.StringValue : "",
                    Category = p.TryGetValue("category", out Value? ca) && Enum.TryParse<IntentionCategory>(ca.StringValue, out IntentionCategory cv) ? cv : IntentionCategory.SelfReflection,
                    Source = p.TryGetValue("source", out Value? s) ? s.StringValue : "",
                    Priority = p.TryGetValue("priority", out Value? pr) && Enum.TryParse<IntentionPriority>(pr.IntegerValue.ToString(), out IntentionPriority pv) ? pv : IntentionPriority.Normal,
                };
            }).ToList();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Qdrant RPC search error: {ex.Status.Detail}");
            return Array.Empty<Intention>();
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search failed: {ex.Message}");
            return Array.Empty<Intention>();
        }
    }

    /// <summary>
    /// Gets collection statistics.
    /// </summary>
    public async Task<QdrantCollectionStats> GetCollectionStatsAsync(string collectionName, CancellationToken ct = default)
    {
        try
        {
            bool exists = await _client.CollectionExistsAsync(collectionName, ct);
            if (!exists)
            {
                return new QdrantCollectionStats { Name = collectionName, Exists = false };
            }

            CollectionInfo info = await _client.GetCollectionInfoAsync(collectionName, ct);
            ulong count = await _client.CountAsync(collectionName, exact: true, cancellationToken: ct);
            int vectorSize = (int)(info.Config?.Params?.VectorsConfig?.Params?.Size ?? 0);

            return new QdrantCollectionStats
            {
                Name = collectionName,
                Exists = true,
                PointCount = (long)count,
                VectorSize = vectorSize,
            };
        }
        catch (Grpc.Core.RpcException)
        {
            return new QdrantCollectionStats { Name = collectionName, Exists = false };
        }
    }

    /// <summary>
    /// Gets statistics for all neural memory collections.
    /// </summary>
    public async Task<QdrantNeuralMemoryStats> GetStatsAsync(CancellationToken ct = default)
    {
        QdrantCollectionStats messagesStats = await GetCollectionStatsAsync(_neuronMessagesCollection, ct);
        QdrantCollectionStats intentionsStats = await GetCollectionStatsAsync(_intentionsCollection, ct);
        QdrantCollectionStats memoriesStats = await GetCollectionStatsAsync(_memoriesCollection, ct);

        return new QdrantNeuralMemoryStats
        {
            IsConnected = messagesStats.Exists || intentionsStats.Exists || memoriesStats.Exists,
            NeuronMessagesCount = messagesStats.PointCount,
            IntentionsCount = intentionsStats.PointCount,
            MemoriesCount = memoriesStats.PointCount,
            TotalPoints = messagesStats.PointCount + intentionsStats.PointCount + memoriesStats.PointCount,
        };
    }
}
