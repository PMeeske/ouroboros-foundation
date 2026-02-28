// <copyright file="QdrantNeuralMemory.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Text.Json;
using Google.Protobuf.Collections;
using Ouroboros.Core.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Provides Qdrant-based persistent memory for the neural network.
/// Stores neuron messages, intentions, and enables semantic search.
/// Uses the shared IQdrantClient via gRPC for unified Qdrant access.
/// </summary>
public sealed partial class QdrantNeuralMemory : IDisposable
{
    private readonly QdrantClient _client;
    private readonly string _neuronMessagesCollection;
    private readonly string _intentionsCollection;
    private readonly string _memoriesCollection;
    private readonly int _defaultVectorSize;
    private readonly ConcurrentDictionary<string, bool> _initializedCollections = new();
    private readonly bool _disposeClient;

    /// <summary>
    /// Creates a new Qdrant neural memory instance using DI-provided client and collection registry.
    /// </summary>
    /// <param name="client">Shared Qdrant client from DI.</param>
    /// <param name="registry">Collection registry for role-based resolution.</param>
    /// <param name="settings">Qdrant settings for vector size.</param>
    public QdrantNeuralMemory(
        QdrantClient client,
        IQdrantCollectionRegistry registry,
        QdrantSettings settings)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(settings);
        _neuronMessagesCollection = registry.GetCollectionName(QdrantCollectionRole.NeuronMessages);
        _intentionsCollection = registry.GetCollectionName(QdrantCollectionRole.Intentions);
        _memoriesCollection = registry.GetCollectionName(QdrantCollectionRole.Memories);
        _defaultVectorSize = settings.DefaultVectorSize;
        _disposeClient = false;
    }

    /// <summary>
    /// Creates a new Qdrant neural memory instance.
    /// </summary>
    /// <param name="qdrantEndpoint">Qdrant gRPC endpoint (e.g., http://localhost:6334).</param>
    [Obsolete("Use the constructor accepting QdrantClient + IQdrantCollectionRegistry from DI.")]
    public QdrantNeuralMemory(string qdrantEndpoint = DefaultEndpoints.QdrantGrpc)
    {
        Uri uri = new Uri(qdrantEndpoint.TrimEnd('/'));
        _client = new QdrantClient(uri.Host, uri.Port > 0 ? uri.Port : 6334, uri.Scheme == "https");
        _neuronMessagesCollection = "ouroboros_neuron_messages";
        _intentionsCollection = "ouroboros_intentions";
        _memoriesCollection = "ouroboros_memories";
        _defaultVectorSize = 768;
        _disposeClient = true;
    }

    /// <summary>
    /// Delegate for embedding text.
    /// </summary>
    public Func<string, CancellationToken, Task<float[]>>? EmbedFunction { get; set; }

    /// <summary>
    /// Initializes all required collections in Qdrant.
    /// Automatically handles dimension mismatches by migrating data.
    /// </summary>
    public async Task InitializeAsync(int vectorSize = 0, CancellationToken ct = default)
    {
        int size = vectorSize > 0 ? vectorSize : _defaultVectorSize;
        await EnsureCollectionWithMigrationAsync(_neuronMessagesCollection, size, ct);
        await EnsureCollectionWithMigrationAsync(_intentionsCollection, size, ct);
        await EnsureCollectionWithMigrationAsync(_memoriesCollection, size, ct);
    }

    /// <summary>
    /// Ensures a collection exists with the correct vector size, migrating data if needed.
    /// </summary>
    private async Task EnsureCollectionWithMigrationAsync(string collectionName, int vectorSize, CancellationToken ct)
    {
        if (_initializedCollections.ContainsKey(collectionName)) return;

        try
        {
            bool exists = await _client.CollectionExistsAsync(collectionName, ct);

            if (!exists)
            {
                await _client.CreateCollectionAsync(
                    collectionName,
                    new VectorParams { Size = (ulong)vectorSize, Distance = Distance.Cosine },
                    cancellationToken: ct);
                _initializedCollections[collectionName] = true;
                return;
            }

            // Check dimension
            CollectionInfo info = await _client.GetCollectionInfoAsync(collectionName, ct);
            int currentSize = (int)(info.Config?.Params?.VectorsConfig?.Params?.Size ?? 0);

            if (currentSize == vectorSize)
            {
                _initializedCollections[collectionName] = true;
                return;
            }

            // Dimension mismatch - need to migrate
            Console.WriteLine($"  \u26a0 Qdrant: {collectionName} dimension mismatch ({currentSize} \u2192 {vectorSize})");

            ulong pointCount = await _client.CountAsync(collectionName, exact: true, cancellationToken: ct);

            if (pointCount == 0)
            {
                Console.WriteLine($"    Recreating empty collection...");
                await _client.DeleteCollectionAsync(collectionName, cancellationToken: ct);
                await _client.CreateCollectionAsync(
                    collectionName,
                    new VectorParams { Size = (ulong)vectorSize, Distance = Distance.Cosine },
                    cancellationToken: ct);
            }
            else if (EmbedFunction != null)
            {
                Console.WriteLine($"    Migrating {pointCount} points with new embeddings...");
                await MigrateCollectionAsync(collectionName, vectorSize, ct);
            }
            else
            {
                Console.WriteLine($"    \u26a0 Cannot re-embed {pointCount} points (no embedding function)");
                Console.WriteLine($"    Creating backup and recreating collection...");
                Console.WriteLine($"    \u26a0 Data in {collectionName} will be reset");
                await _client.DeleteCollectionAsync(collectionName, cancellationToken: ct);
                await _client.CreateCollectionAsync(
                    collectionName,
                    new VectorParams { Size = (ulong)vectorSize, Distance = Distance.Cosine },
                    cancellationToken: ct);
            }

            _initializedCollections[collectionName] = true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Qdrant RPC error ensuring collection {collectionName}: {ex.Status.Detail}");
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to ensure collection {collectionName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Migrates a collection to a new vector size by re-embedding all content.
    /// </summary>
    private async Task MigrateCollectionAsync(string collectionName, int newVectorSize, CancellationToken ct)
    {
        try
        {
            // 1. Scroll through all points and extract payloads
            List<(string Id, Dictionary<string, string> Payload)> allPayloads = await ScrollAllPayloadsAsync(collectionName, ct);
            Console.WriteLine($"    Retrieved {allPayloads.Count} payloads for migration");

            // 2. Delete old collection
            await _client.DeleteCollectionAsync(collectionName, cancellationToken: ct);

            // 3. Create new collection with correct dimension
            await _client.CreateCollectionAsync(
                collectionName,
                new VectorParams { Size = (ulong)newVectorSize, Distance = Distance.Cosine },
                cancellationToken: ct);

            // 4. Re-embed and insert each point
            int migrated = 0;
            int failed = 0;
            foreach ((string? id, Dictionary<string, string>? payload) in allPayloads)
            {
                try
                {
                    string searchText = BuildSearchTextFromPayload(collectionName, payload);
                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        failed++;
                        continue;
                    }

                    float[] newEmbedding = await EmbedFunction!(searchText, ct);
                    if (newEmbedding == null || newEmbedding.Length != newVectorSize)
                    {
                        failed++;
                        continue;
                    }

                    PointStruct point = new PointStruct
                    {
                        Id = new PointId { Uuid = id },
                        Vectors = newEmbedding,
                    };
                    foreach ((string? key, string? value) in payload)
                    {
                        point.Payload[key] = value?.ToString() ?? string.Empty;
                    }

                    await _client.UpsertAsync(collectionName, new[] { point }, cancellationToken: ct);
                    migrated++;
                }
                catch (Grpc.Core.RpcException)
                {
                    failed++;
                }
            }

            Console.WriteLine($"    \u2713 Migrated {migrated} points ({failed} failed)");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            Console.WriteLine($"    \u26a0 Migration RPC error: {ex.Status.Detail}");
            await _client.CreateCollectionAsync(
                collectionName,
                new VectorParams { Size = (ulong)newVectorSize, Distance = Distance.Cosine },
                cancellationToken: ct);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"    \u26a0 Migration failed: {ex.Message}");
            await _client.CreateCollectionAsync(
                collectionName,
                new VectorParams { Size = (ulong)newVectorSize, Distance = Distance.Cosine },
                cancellationToken: ct);
        }
    }

    private string BuildSearchTextFromPayload(string collectionName, Dictionary<string, string> payload)
    {
        if (collectionName == _neuronMessagesCollection)
            return $"{payload.GetValueOrDefault("topic")} {payload.GetValueOrDefault("content")}";
        if (collectionName == _intentionsCollection)
            return $"{payload.GetValueOrDefault("title")} {payload.GetValueOrDefault("description")} {payload.GetValueOrDefault("rationale")}";
        if (collectionName == _memoriesCollection)
            return $"{payload.GetValueOrDefault("category")} {payload.GetValueOrDefault("content")}";
        return payload.GetValueOrDefault("content") ?? "";
    }

    /// <summary>
    /// Scrolls through all points in a collection and returns their payloads.
    /// </summary>
    private async Task<List<(string Id, Dictionary<string, string> Payload)>> ScrollAllPayloadsAsync(
        string collectionName, CancellationToken ct)
    {
        List<(string, Dictionary<string, string>)> results = new List<(string, Dictionary<string, string>)>();
        PointId? offset = null;

        while (true)
        {
            try
            {
                ScrollResponse scrollResult = await _client.ScrollAsync(
                    collectionName,
                    limit: 100,
                    offset: offset,
                    payloadSelector: new WithPayloadSelector { Enable = true },
                    cancellationToken: ct);

                foreach (RetrievedPoint? point in scrollResult.Result)
                {
                    string id = point.Id.Uuid ?? point.Id.Num.ToString();
                    Dictionary<string, string> payloadDict = new Dictionary<string, string>();

                    foreach (KeyValuePair<string, Value> kvp in point.Payload)
                    {
                        payloadDict[kvp.Key] = kvp.Value.StringValue ?? kvp.Value.ToString();
                    }

                    results.Add((id, payloadDict));
                }

                offset = scrollResult.Result.Count > 0 ? scrollResult.Result.Last().Id : null;
                if (offset == null) break;
            }
            catch (Grpc.Core.RpcException)
            {
                break;
            }
        }

        return results;
    }

    /// <summary>
    /// Stores a neuron message in Qdrant for persistence and search.
    /// </summary>
    public async Task StoreNeuronMessageAsync(NeuronMessage message, CancellationToken ct = default)
    {
        string content = JsonSerializer.Serialize(message.Payload);
        string searchText = $"{message.Topic} {content}";

        float[]? embedding = await GetEmbeddingAsync(searchText, ct);
        if (embedding == null) return;

        PointStruct point = new PointStruct
        {
            Id = new PointId { Uuid = message.Id.ToString() },
            Vectors = embedding,
            Payload =
            {
                ["source_neuron"] = message.SourceNeuron,
                ["target_neuron"] = message.TargetNeuron ?? "",
                ["topic"] = message.Topic,
                ["content"] = content,
                ["priority"] = (long)(int)message.Priority,
                ["created_at"] = message.CreatedAt.ToString("O"),
                ["ttl_seconds"] = (long)message.TtlSeconds,
            }
        };

        await _client.UpsertAsync(_neuronMessagesCollection, new[] { point }, cancellationToken: ct);
    }

    /// <summary>
    /// Stores an intention in Qdrant.
    /// </summary>
    public async Task StoreIntentionAsync(Intention intention, CancellationToken ct = default)
    {
        string searchText = $"{intention.Title} {intention.Description} {intention.Rationale}";

        float[]? embedding = await GetEmbeddingAsync(searchText, ct);
        if (embedding == null) return;

        PointStruct point = new PointStruct
        {
            Id = new PointId { Uuid = intention.Id.ToString() },
            Vectors = embedding,
            Payload =
            {
                ["title"] = intention.Title,
                ["description"] = intention.Description,
                ["rationale"] = intention.Rationale,
                ["category"] = intention.Category.ToString(),
                ["priority"] = (long)(int)intention.Priority,
                ["source"] = intention.Source,
                ["status"] = intention.Status.ToString(),
                ["created_at"] = intention.CreatedAt.ToString("O"),
                ["requires_approval"] = intention.RequiresApproval,
            }
        };

        await _client.UpsertAsync(_intentionsCollection, new[] { point }, cancellationToken: ct);
    }

    /// <summary>
    /// Stores a memory/fact in Qdrant.
    /// </summary>
    public async Task StoreMemoryAsync(string category, string content, float[] embedding, CancellationToken ct = default)
    {
        string id = Guid.NewGuid().ToString();

        PointStruct point = new PointStruct
        {
            Id = new PointId { Uuid = id },
            Vectors = embedding,
            Payload =
            {
                ["category"] = category,
                ["content"] = content,
                ["created_at"] = DateTime.UtcNow.ToString("O"),
            }
        };

        await _client.UpsertAsync(_memoriesCollection, new[] { point }, cancellationToken: ct);
    }

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

    private async Task<float[]?> GetEmbeddingAsync(string text, CancellationToken ct)
    {
        if (EmbedFunction == null) return null;
        return await EmbedFunction(text, ct);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposeClient)
        {
            _client.Dispose();
        }
    }
}
