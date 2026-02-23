// <copyright file="QdrantCollectionAdmin.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Ouroboros's self-administered Qdrant collection manager.
/// Provides capabilities for managing, linking, and self-healing vector collections.
/// </summary>
public sealed class QdrantCollectionAdmin : IAsyncDisposable
{
    private const int DefaultVectorSize = 768; // nomic-embed-text

    private readonly QdrantClient _client;
    private readonly IQdrantCollectionRegistry? _registry;
    private readonly string _endpoint;
    private readonly bool _disposeClient;
    private readonly Dictionary<string, CollectionInfo> _collectionCache = new();
    private readonly List<CollectionLink> _collectionLinks = new();
    private bool _initialized;

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
        _endpoint = "di-managed";
        _disposeClient = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantCollectionAdmin"/> class.
    /// </summary>
    /// <param name="endpoint">Qdrant endpoint (e.g., "http://localhost:6333").</param>
    [Obsolete("Use the constructor accepting QdrantClient + IQdrantCollectionRegistry from DI.")]
    public QdrantCollectionAdmin(string endpoint = "http://localhost:6333")
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        var uri = new Uri(endpoint);
        _client = new QdrantClient(uri.Host, uri.Port > 0 ? uri.Port : 6334, uri.Scheme == "https");
        _disposeClient = true;
    }

    /// <summary>
    /// Initializes a new instance with an existing client.
    /// </summary>
    public QdrantCollectionAdmin(QdrantClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _endpoint = "external";
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

        // Load existing links — prefer registry-driven links when available
        if (_registry != null)
        {
            _collectionLinks.AddRange(GetDefaultLinksFromRegistry());
        }
        else
        {
            _collectionLinks.AddRange(DefaultLinks);
        }

        // Scan existing collections
        await RefreshCollectionCacheAsync(ct);

        _initialized = true;
    }

    /// <summary>
    /// Gets known collections using the registry when available, falling back to static defaults.
    /// </summary>
    public IReadOnlyDictionary<string, string> GetKnownCollections()
    {
        if (_registry == null) return KnownCollections;

        var mappings = _registry.GetAllMappings();
        return mappings.ToDictionary(
            kvp => kvp.Value,
            kvp => KnownCollections.GetValueOrDefault(kvp.Value, kvp.Key.ToString()));
    }

    private IReadOnlyList<CollectionLink> GetDefaultLinksFromRegistry()
    {
        string R(QdrantCollectionRole role) =>
            _registry?.GetCollectionName(role)
            ?? QdrantCollectionRegistry.Defaults.GetValueOrDefault(role, role.ToString());

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
            var exists = await _client.CollectionExistsAsync(collectionName, ct);
            if (!exists) return null;

            var info = await _client.GetCollectionInfoAsync(collectionName, ct);
            var vectorSize = info.Config?.Params?.VectorsConfig?.Params?.Size ?? 0;
            var pointsCount = info.PointsCount;
            var distance = info.Config?.Params?.VectorsConfig?.Params?.Distance ?? Distance.Cosine;
            var status = info.Status;

            GetKnownCollections().TryGetValue(collectionName, out var purpose);
            var links = _collectionLinks
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
        catch
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
            var exists = await _client.CollectionExistsAsync(collectionName, ct);
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
        catch
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
            var exists = await _client.CollectionExistsAsync(collectionName, ct);
            if (!exists) return false;

            await _client.DeleteCollectionAsync(collectionName, cancellationToken: ct);
            _collectionCache.Remove(collectionName);

            // Remove associated links
            _collectionLinks.RemoveAll(l =>
                l.SourceCollection == collectionName || l.TargetCollection == collectionName);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Performs a health check on all collections, detecting dimension mismatches.
    /// </summary>
    public async Task<IReadOnlyList<CollectionHealthReport>> HealthCheckAsync(
        int expectedDimension = DefaultVectorSize,
        CancellationToken ct = default)
    {
        var reports = new List<CollectionHealthReport>();
        await RefreshCollectionCacheAsync(ct);

        foreach (var (name, info) in _collectionCache)
        {
            var mismatch = info.VectorSize != (ulong)expectedDimension && info.VectorSize > 0;
            var issue = mismatch
                ? $"Dimension mismatch: expected {expectedDimension}, got {info.VectorSize}"
                : null;
            var recommendation = mismatch
                ? $"Delete and recreate collection, or migrate vectors to {expectedDimension} dimensions"
                : null;

            reports.Add(new CollectionHealthReport(
                name,
                !mismatch && info.Status == CollectionStatus.Green,
                (ulong)expectedDimension,
                info.VectorSize,
                mismatch,
                issue,
                recommendation));
        }

        return reports.AsReadOnly();
    }

    /// <summary>
    /// Auto-heals collections with dimension mismatches by recreating them.
    /// WARNING: This will delete all data in mismatched collections!
    /// </summary>
    public async Task<IReadOnlyList<string>> AutoHealDimensionMismatchesAsync(
        int targetDimension = DefaultVectorSize,
        CancellationToken ct = default)
    {
        var healed = new List<string>();
        var healthReports = await HealthCheckAsync(targetDimension, ct);

        foreach (var report in healthReports.Where(r => r.DimensionMismatch))
        {
            try
            {
                // Get existing info for recreation
                var info = _collectionCache.GetValueOrDefault(report.CollectionName);
                var distance = info?.DistanceMetric ?? Distance.Cosine;

                // Delete the mismatched collection
                await _client.DeleteCollectionAsync(report.CollectionName, cancellationToken: ct);

                // Recreate with correct dimensions
                await _client.CreateCollectionAsync(
                    report.CollectionName,
                    new VectorParams { Size = (ulong)targetDimension, Distance = distance },
                    cancellationToken: ct);

                healed.Add(report.CollectionName);
            }
            catch
            {
                // Skip collections that can't be healed
            }
        }

        // Refresh cache after healing
        await RefreshCollectionCacheAsync(ct);

        return healed.AsReadOnly();
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

    /// <summary>
    /// Generates a memory map showing collection relationships.
    /// </summary>
    public async Task<string> GenerateMemoryMapAsync(CancellationToken ct = default)
    {
        await RefreshCollectionCacheAsync(ct);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("╔══════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║              OUROBOROS MEMORY ARCHITECTURE                       ║");
        sb.AppendLine("╠══════════════════════════════════════════════════════════════════╣");

        // Group collections by purpose
        var thoughtCollections = _collectionCache.Keys.Where(k => k.Contains("thought")).ToList();
        var skillCollections = _collectionCache.Keys.Where(k => k.Contains("skill") || k.Contains("tool")).ToList();
        var knowledgeCollections = _collectionCache.Keys.Where(k => k.Contains("core") || k.Contains("code")).ToList();
        var personalityCollections = _collectionCache.Keys.Where(k => k.Contains("person") || k.Contains("self")).ToList();
        var otherCollections = _collectionCache.Keys
            .Except(thoughtCollections)
            .Except(skillCollections)
            .Except(knowledgeCollections)
            .Except(personalityCollections)
            .ToList();

        void AppendSection(string title, List<string> collections)
        {
            if (!collections.Any()) return;
            sb.AppendLine($"║ {title,-64} ║");
            foreach (var col in collections)
            {
                var info = _collectionCache.GetValueOrDefault(col);
                var status = info?.Status == CollectionStatus.Green ? "✓" : "⚠";
                var points = info?.PointsCount ?? 0;
                var dim = info?.VectorSize ?? 0;
                sb.AppendLine($"║   {status} {col,-40} [{dim,4}d] {points,8} pts ║");
            }
        }

        AppendSection("🧠 THOUGHT SYSTEM", thoughtCollections);
        AppendSection("🛠️ SKILLS & TOOLS", skillCollections);
        AppendSection("📚 KNOWLEDGE BASE", knowledgeCollections);
        AppendSection("👤 PERSONALITY & SELF", personalityCollections);
        AppendSection("📦 OTHER", otherCollections);

        sb.AppendLine("╠══════════════════════════════════════════════════════════════════╣");
        sb.AppendLine("║ COLLECTION LINKS                                                 ║");

        foreach (var link in _collectionLinks.Take(10))
        {
            sb.AppendLine($"║   {link.SourceCollection,-25} ─{link.RelationType,12}→ {link.TargetCollection,-15} ║");
        }

        if (_collectionLinks.Count > 10)
        {
            sb.AppendLine($"║   ... and {_collectionLinks.Count - 10} more links                                    ║");
        }

        sb.AppendLine("╚══════════════════════════════════════════════════════════════════╝");

        return sb.ToString();
    }

    /// <summary>
    /// Gets statistics about Ouroboros's memory usage.
    /// </summary>
    public async Task<MemoryStatistics> GetMemoryStatisticsAsync(CancellationToken ct = default)
    {
        await RefreshCollectionCacheAsync(ct);

        var totalCollections = _collectionCache.Count;
        var totalPoints = _collectionCache.Values.Sum(c => (long)c.PointsCount);
        var healthyCollections = _collectionCache.Values.Count(c => c.Status == CollectionStatus.Green);
        var dimensionGroups = _collectionCache.Values
            .GroupBy(c => c.VectorSize)
            .ToDictionary(g => g.Key, g => g.Count());

        return new MemoryStatistics(
            totalCollections,
            totalPoints,
            healthyCollections,
            totalCollections - healthyCollections,
            _collectionLinks.Count,
            dimensionGroups);
    }

    private async Task RefreshCollectionCacheAsync(CancellationToken ct)
    {
        try
        {
            var collections = await _client.ListCollectionsAsync(ct);
            _collectionCache.Clear();

            foreach (var collectionName in collections)
            {
                var info = await GetCollectionInfoAsync(collectionName, ct);
                if (info != null)
                {
                    _collectionCache[collectionName] = info;
                }
            }
        }
        catch
        {
            // Keep existing cache on error
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposeClient)
        {
            _client.Dispose();
        }

        await Task.CompletedTask;
    }
}