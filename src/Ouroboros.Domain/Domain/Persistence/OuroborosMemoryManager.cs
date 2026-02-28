// <copyright file="OuroborosMemoryManager.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Ouroboros's unified memory management system.
/// Provides a cognitive architecture over the underlying vector collections.
/// </summary>
public sealed class OuroborosMemoryManager : IAsyncDisposable
{
    private readonly QdrantCollectionAdmin _admin;
    private readonly Dictionary<MemoryLayer, MemoryLayerMapping> _layerMappings;
    private bool _initialized;

    /// <summary>
    /// Default memory layer mappings for Ouroboros.
    /// </summary>
    public static readonly IReadOnlyList<MemoryLayerMapping> DefaultLayerMappings = new List<MemoryLayerMapping>
    {
        new(MemoryLayer.Working, new[] { "ouroboros_neuro_thoughts" },
            "Active thought processes and immediate reasoning", 1.0),

        new(MemoryLayer.Episodic, new[] { "ouroboros_conversations", "ouroboros_thought_results" },
            "Recent interactions and their outcomes", 0.9),

        new(MemoryLayer.Semantic, new[] { "core", "fullcore", "codebase", "qdrant_documentation" },
            "Learned facts, concepts, and domain knowledge", 0.7),

        new(MemoryLayer.Procedural, new[] { "ouroboros_skills", "ouroboros_tool_patterns", "tools" },
            "Learned skills, tool usage patterns, and procedures", 0.8),

        new(MemoryLayer.Autobiographical, new[] { "ouroboros_personalities", "ouroboros_persons", "ouroboros_selfindex" },
            "Self-model, identity, and known entities", 0.95),
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="OuroborosMemoryManager"/> class.
    /// </summary>
    /// <param name="qdrantEndpoint">Qdrant server endpoint.</param>
    public OuroborosMemoryManager(string qdrantEndpoint = DefaultEndpoints.Qdrant)
    {
        _admin = new QdrantCollectionAdmin(qdrantEndpoint);
        _layerMappings = DefaultLayerMappings.ToDictionary(m => m.Layer);
    }

    /// <summary>
    /// Gets the underlying collection admin.
    /// </summary>
    public QdrantCollectionAdmin Admin => _admin;

    /// <summary>
    /// Initializes the memory system, ensuring all required collections exist.
    /// </summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_initialized) return;

        await _admin.InitializeAsync(ct);

        // Ensure all memory layer collections exist
        foreach (MemoryLayerMapping mapping in _layerMappings.Values)
        {
            foreach (string collection in mapping.Collections)
            {
                CollectionInfo? info = await _admin.GetCollectionInfoAsync(collection, ct);
                if (info == null)
                {
                    // Create missing collection
                    QdrantCollectionAdmin.KnownCollections.TryGetValue(collection, out string? purpose);
                    await _admin.CreateCollectionAsync(collection, purpose: purpose ?? mapping.Description, ct: ct);
                }
            }
        }

        _initialized = true;
    }

    /// <summary>
    /// Performs a full memory health check and auto-heals issues.
    /// </summary>
    public async Task<MemoryHealthReport> PerformHealthCheckAsync(
        bool autoHeal = false,
        CancellationToken ct = default)
    {
        IReadOnlyList<CollectionHealthReport> healthReports = await _admin.HealthCheckAsync(ct: ct);
        List<CollectionHealthReport> unhealthyCollections = healthReports.Where(r => !r.IsHealthy).ToList();
        List<string> healedCollections = new List<string>();

        if (autoHeal && unhealthyCollections.Any())
        {
            healedCollections = (await _admin.AutoHealDimensionMismatchesAsync(ct: ct)).ToList();
        }

        MemoryStatistics stats = await _admin.GetMemoryStatisticsAsync(ct);

        return new MemoryHealthReport(
            healthReports.Count(r => r.IsHealthy),
            unhealthyCollections.Count,
            healedCollections,
            unhealthyCollections.Select(r => r.CollectionName).ToList(),
            stats);
    }

    /// <summary>
    /// Gets collections for a specific memory layer.
    /// </summary>
    public IReadOnlyList<string> GetCollectionsForLayer(MemoryLayer layer)
    {
        return _layerMappings.TryGetValue(layer, out MemoryLayerMapping? mapping)
            ? mapping.Collections
            : Array.Empty<string>();
    }

    /// <summary>
    /// Determines which memory layer a collection belongs to.
    /// </summary>
    public MemoryLayer? GetLayerForCollection(string collectionName)
    {
        foreach ((MemoryLayer layer, MemoryLayerMapping? mapping) in _layerMappings)
        {
            if (mapping.Collections.Contains(collectionName))
            {
                return layer;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the memory map visualization.
    /// </summary>
    public async Task<string> GetMemoryMapAsync(CancellationToken ct = default)
    {
        return await _admin.GenerateMemoryMapAsync(ct);
    }

    /// <summary>
    /// Gets total vector count across all memory layers.
    /// </summary>
    public async Task<long> GetTotalMemoryVectorsAsync(CancellationToken ct = default)
    {
        MemoryStatistics stats = await _admin.GetMemoryStatisticsAsync(ct);
        return stats.TotalVectors;
    }

    /// <summary>
    /// Gets vector count for a specific memory layer.
    /// </summary>
    public async Task<long> GetLayerVectorCountAsync(MemoryLayer layer, CancellationToken ct = default)
    {
        IReadOnlyList<string> collections = GetCollectionsForLayer(layer);
        long total = 0;

        foreach (string collection in collections)
        {
            CollectionInfo? info = await _admin.GetCollectionInfoAsync(collection, ct);
            if (info != null)
            {
                total += (long)info.PointsCount;
            }
        }

        return total;
    }

    /// <summary>
    /// Clears a specific memory layer (with confirmation).
    /// </summary>
    public async Task<bool> ClearMemoryLayerAsync(
        MemoryLayer layer,
        bool confirmed = false,
        CancellationToken ct = default)
    {
        if (!confirmed) return false;

        IReadOnlyList<string> collections = GetCollectionsForLayer(layer);
        bool success = true;

        foreach (string collection in collections)
        {
            bool deleted = await _admin.DeleteCollectionAsync(collection, ct);
            if (deleted)
            {
                // Recreate empty collection
                QdrantCollectionAdmin.KnownCollections.TryGetValue(collection, out string? purpose);
                await _admin.CreateCollectionAsync(collection, purpose: purpose, ct: ct);
            }
            else
            {
                success = false;
            }
        }

        return success;
    }

    /// <summary>
    /// Creates a snapshot of the memory system state.
    /// </summary>
    public async Task<MemorySnapshot> CreateSnapshotAsync(CancellationToken ct = default)
    {
        IReadOnlyList<CollectionInfo> collections = await _admin.GetAllCollectionsAsync(ct);
        MemoryStatistics stats = await _admin.GetMemoryStatisticsAsync(ct);
        IReadOnlyList<CollectionLink> links = _admin.CollectionLinks;

        Dictionary<MemoryLayer, long> layerStats = new Dictionary<MemoryLayer, long>();
        foreach (MemoryLayer layer in Enum.GetValues<MemoryLayer>())
        {
            layerStats[layer] = await GetLayerVectorCountAsync(layer, ct);
        }

        return new MemorySnapshot(
            DateTime.UtcNow,
            collections.ToList(),
            links.ToList(),
            layerStats,
            stats);
    }

    /// <summary>
    /// Links two collections in the memory graph.
    /// </summary>
    public void LinkCollections(string source, string target, string relationType, string? description = null)
    {
        _admin.AddCollectionLink(new CollectionLink(source, target, relationType, Description: description));
    }

    /// <summary>
    /// Gets all collections related to a given collection.
    /// </summary>
    public IReadOnlyList<CollectionLink> GetRelatedCollections(string collectionName)
    {
        return _admin.GetLinkedCollections(collectionName);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await _admin.DisposeAsync();
    }
}