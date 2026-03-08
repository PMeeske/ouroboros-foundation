// <copyright file="QdrantCollectionAdmin.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Text;
using Ouroboros.Core.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Ouroboros's self-administered Qdrant collection manager.
/// Provides capabilities for managing, linking, and self-healing vector collections.
/// </summary>
public sealed partial class QdrantCollectionAdmin : IAsyncDisposable
{
    private const int DefaultVectorSize = 768; // nomic-embed-text

    private readonly QdrantClient _client;
    private readonly IQdrantCollectionRegistry _registry;
    private readonly bool _disposeClient;
    private readonly Dictionary<string, CollectionInfo> _collectionCache = new();
    private readonly List<CollectionLink> _collectionLinks = new();
    private bool _initialized;
    private bool _disposed;

    /// <summary>
    /// Known Ouroboros collections and their purposes.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> KnownCollections = new Dictionary<string, string>
    {
        ["ouroboros_neuro_thoughts"] = "Neural-symbolic thought storage for inner dialog",
        ["ouroboros_thought_relations"] = "Symbolic relations between thoughts",
        ["ouroboros_thought_results"] = "Outcomes and results of thought chains",
        ["ouroboros_conversations"] = "Conversation history and context",
        ["ouroboros_skills"] = "Learned skills and capabilities",
        ["ouroboros_tool_patterns"] = "Tool usage patterns and preferences",
        ["ouroboros_personalities"] = "Personality trait vectors",
        ["ouroboros_persons"] = "Known persons and their attributes",
        ["ouroboros_selfindex"] = "Self-referential knowledge index",
        ["ouroboros_filehashes"] = "File content hashes for deduplication",
        ["pipeline_vectors"] = "General pipeline vector storage",
        ["tools"] = "Tool definitions and embeddings",
        ["core"] = "Core knowledge embeddings",
        ["fullcore"] = "Full codebase embeddings",
        ["codebase"] = "Source code embeddings",
        ["prefix_cache"] = "Prefix-based completion cache",
        ["qdrant_documentation"] = "Qdrant documentation embeddings",
    };

    /// <summary>
    /// Default collection links representing Ouroboros's memory architecture.
    /// </summary>
    public static readonly IReadOnlyList<CollectionLink> DefaultLinks = new List<CollectionLink>
    {
        new("ouroboros_neuro_thoughts", "ouroboros_thought_relations", CollectionLink.Types.Indexes, 1.0, "Thoughts indexed by relations"),
        new("ouroboros_neuro_thoughts", "ouroboros_thought_results", CollectionLink.Types.Extends, 1.0, "Thoughts extend to results"),
        new("ouroboros_skills", "ouroboros_tool_patterns", CollectionLink.Types.RelatedTo, 0.8, "Skills inform tool patterns"),
        new("ouroboros_conversations", "ouroboros_neuro_thoughts", CollectionLink.Types.DependsOn, 0.9, "Conversations feed thoughts"),
        new("ouroboros_personalities", "ouroboros_persons", CollectionLink.Types.RelatedTo, 0.7, "Personalities relate to persons"),
        new("ouroboros_selfindex", "ouroboros_neuro_thoughts", CollectionLink.Types.Aggregates, 1.0, "Self-index aggregates thoughts"),
        new("core", "fullcore", CollectionLink.Types.PartOf, 1.0, "Core is part of fullcore"),
        new("codebase", "fullcore", CollectionLink.Types.PartOf, 1.0, "Codebase is part of fullcore"),
    };

    /// <summary>
    /// Initializes a new instance using the DI-provided client and collection registry.
    /// </summary>
    /// <param name="client">Shared Qdrant client from DI.</param>
    /// <param name="registry">Collection registry for role-based resolution.</param>
    public QdrantCollectionAdmin(QdrantClient client, IQdrantCollectionRegistry registry)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _disposeClient = false;
    }

    /// <summary>
    /// Gets all collection links.
    /// </summary>
    public IReadOnlyList<CollectionLink> CollectionLinks => _collectionLinks.AsReadOnly();

    /// <summary>
    /// Initializes the admin service and loads collection metadata.
    /// </summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_initialized) return;

        // Load existing links from registry
        _collectionLinks.AddRange(GetDefaultLinksFromRegistry());

        // Scan existing collections
        await RefreshCollectionCacheAsync(ct);

        _initialized = true;
    }

    /// <summary>
    /// Gets known collections using the registry when available, falling back to static defaults.
    /// </summary>
    public IReadOnlyDictionary<string, string> GetKnownCollections()
    {
        IReadOnlyDictionary<QdrantCollectionRole, string> mappings = _registry.GetAllMappings();
        return mappings.ToDictionary(
            kvp => kvp.Value,
            kvp => KnownCollections.GetValueOrDefault(kvp.Value, kvp.Key.ToString()));
    }

    private IReadOnlyList<CollectionLink> GetDefaultLinksFromRegistry()
    {
        string R(QdrantCollectionRole role) =>
            _registry.GetCollectionName(role);

        return new List<CollectionLink>
        {
            new(R(QdrantCollectionRole.NeuroThoughts), R(QdrantCollectionRole.ThoughtRelations), CollectionLink.Types.Indexes, 1.0, "Thoughts indexed by relations"),
            new(R(QdrantCollectionRole.NeuroThoughts), R(QdrantCollectionRole.ThoughtResults), CollectionLink.Types.Extends, 1.0, "Thoughts extend to results"),
            new(R(QdrantCollectionRole.Skills), R(QdrantCollectionRole.ToolPatterns), CollectionLink.Types.RelatedTo, 0.8, "Skills inform tool patterns"),
            new(R(QdrantCollectionRole.Conversations), R(QdrantCollectionRole.NeuroThoughts), CollectionLink.Types.DependsOn, 0.9, "Conversations feed thoughts"),
            new(R(QdrantCollectionRole.Personalities), R(QdrantCollectionRole.Persons), CollectionLink.Types.RelatedTo, 0.7, "Personalities relate to persons"),
            new(R(QdrantCollectionRole.SelfIndex), R(QdrantCollectionRole.NeuroThoughts), CollectionLink.Types.Aggregates, 1.0, "Self-index aggregates thoughts"),
            new(R(QdrantCollectionRole.Core), R(QdrantCollectionRole.FullCore), CollectionLink.Types.PartOf, 1.0, "Core is part of fullcore"),
            new(R(QdrantCollectionRole.Codebase), R(QdrantCollectionRole.FullCore), CollectionLink.Types.PartOf, 1.0, "Codebase is part of fullcore"),
        };
    }

    /// <summary>
    /// Gets information about all Qdrant collections.
    /// </summary>
    public async Task<IReadOnlyList<CollectionInfo>> GetAllCollectionsAsync(CancellationToken ct = default)
    {
        await RefreshCollectionCacheAsync(ct);
        return _collectionCache.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets information about a specific collection.
    /// </summary>
    public async Task<CollectionInfo?> GetCollectionInfoAsync(string collectionName, CancellationToken ct = default)
    {
        try
        {
            bool exists = await _client.CollectionExistsAsync(collectionName, ct);
            if (!exists) return null;

            Qdrant.Client.Grpc.CollectionInfo info = await _client.GetCollectionInfoAsync(collectionName, ct);
            ulong vectorSize = info.Config?.Params?.VectorsConfig?.Params?.Size ?? 0;
            ulong pointsCount = info.PointsCount;
            Distance distance = info.Config?.Params?.VectorsConfig?.Params?.Distance ?? Distance.Cosine;
            CollectionStatus status = info.Status;

            GetKnownCollections().TryGetValue(collectionName, out string? purpose);
            List<string> links = _collectionLinks
                .Where(l => l.SourceCollection == collectionName || l.TargetCollection == collectionName)
                .Select(l => l.SourceCollection == collectionName ? l.TargetCollection : l.SourceCollection)
                .Distinct()
                .ToList();

            return new CollectionInfo(
                collectionName,
                vectorSize,
                pointsCount,
                distance,
                status,
                Purpose: purpose,
                LinkedCollections: links);
        }
        catch (Grpc.Core.RpcException)
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a new collection with the specified parameters.
    /// </summary>
    public async Task<bool> CreateCollectionAsync(
        string collectionName,
        int vectorSize = DefaultVectorSize,
        Distance distance = Distance.Cosine,
        string? purpose = null,
        CancellationToken ct = default)
    {
        try
        {
            bool exists = await _client.CollectionExistsAsync(collectionName, ct);
            if (exists) return false;

            await _client.CreateCollectionAsync(
                collectionName,
                new VectorParams { Size = (ulong)vectorSize, Distance = distance },
                cancellationToken: ct);

            // Update cache
            _collectionCache[collectionName] = new CollectionInfo(
                collectionName,
                (ulong)vectorSize,
                0,
                distance,
                CollectionStatus.Green,
                DateTime.UtcNow,
                purpose);

            return true;
        }
        catch (Grpc.Core.RpcException)
        {
            return false;
        }
    }

    /// <summary>
    /// Deletes a collection.
    /// </summary>
    public async Task<bool> DeleteCollectionAsync(string collectionName, CancellationToken ct = default)
    {
        try
        {
            bool exists = await _client.CollectionExistsAsync(collectionName, ct);
            if (!exists) return false;

            await _client.DeleteCollectionAsync(collectionName, cancellationToken: ct);
            _collectionCache.Remove(collectionName);

            // Remove associated links
            _collectionLinks.RemoveAll(l =>
                l.SourceCollection == collectionName || l.TargetCollection == collectionName);

            return true;
        }
        catch (Grpc.Core.RpcException)
        {
            return false;
        }
    }

    /// <summary>
    /// Adds a link between two collections.
    /// </summary>
    public void AddCollectionLink(CollectionLink link)
    {
        if (!_collectionLinks.Any(l =>
            l.SourceCollection == link.SourceCollection &&
            l.TargetCollection == link.TargetCollection &&
            l.RelationType == link.RelationType))
        {
            _collectionLinks.Add(link);
        }
    }

    /// <summary>
    /// Gets all collections linked to a specific collection.
    /// </summary>
    public IReadOnlyList<CollectionLink> GetLinkedCollections(string collectionName)
    {
        return _collectionLinks
            .Where(l => l.SourceCollection == collectionName || l.TargetCollection == collectionName)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets collections by relationship type.
    /// </summary>
    public IReadOnlyList<string> GetCollectionsByRelation(string collectionName, string relationType)
    {
        return _collectionLinks
            .Where(l => l.SourceCollection == collectionName && l.RelationType == relationType)
            .Select(l => l.TargetCollection)
            .Concat(_collectionLinks
                .Where(l => l.TargetCollection == collectionName && l.RelationType == relationType)
                .Select(l => l.SourceCollection))
            .Distinct()
            .ToList()
            .AsReadOnly();
    }

    private async Task RefreshCollectionCacheAsync(CancellationToken ct)
    {
        try
        {
            IReadOnlyList<string> collections = await _client.ListCollectionsAsync(ct);
            _collectionCache.Clear();

            foreach (string collectionName in collections)
            {
                CollectionInfo? info = await GetCollectionInfoAsync(collectionName, ct);
                if (info != null)
                {
                    _collectionCache[collectionName] = info;
                }
            }
        }
        catch (Grpc.Core.RpcException)
        {
            // Keep existing cache on error
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (_disposed) return ValueTask.CompletedTask;
        _disposed = true;

        if (_disposeClient)
        {
            _client.Dispose();
        }

        return ValueTask.CompletedTask;
    }
}
