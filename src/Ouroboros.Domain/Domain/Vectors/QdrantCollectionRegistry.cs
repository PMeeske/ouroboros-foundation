using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouroboros.Core.Configuration;
using Qdrant.Client;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Resolves Qdrant collection names by role, seeded from configuration defaults
/// and enriched at startup via live collection discovery.
/// </summary>
public sealed class QdrantCollectionRegistry : IQdrantCollectionRegistry
{
    private readonly ConcurrentDictionary<QdrantCollectionRole, string> _mappings = new();
    private readonly QdrantClient _client;
    private readonly ILogger<QdrantCollectionRegistry>? _logger;

    /// <summary>
    /// Default role-to-collection-name mappings.
    /// </summary>
    public static readonly IReadOnlyDictionary<QdrantCollectionRole, string> Defaults =
        new Dictionary<QdrantCollectionRole, string>
        {
            // Thought System
            [QdrantCollectionRole.NeuroThoughts] = "ouroboros_neuro_thoughts",
            [QdrantCollectionRole.ThoughtRelations] = "ouroboros_thought_relations",
            [QdrantCollectionRole.ThoughtResults] = "ouroboros_thought_results",

            // Neural Memory
            [QdrantCollectionRole.NeuronMessages] = "ouroboros_neuron_messages",
            [QdrantCollectionRole.Intentions] = "ouroboros_intentions",
            [QdrantCollectionRole.Memories] = "ouroboros_memories",

            // Conversations
            [QdrantCollectionRole.Conversations] = "ouroboros_conversations",

            // Skills & Tools
            [QdrantCollectionRole.Skills] = "ouroboros_skills",
            [QdrantCollectionRole.ToolPatterns] = "ouroboros_tool_patterns",
            [QdrantCollectionRole.Tools] = "tools",

            // Knowledge
            [QdrantCollectionRole.Core] = "core",
            [QdrantCollectionRole.FullCore] = "fullcore",
            [QdrantCollectionRole.Codebase] = "codebase",
            [QdrantCollectionRole.PrefixCache] = "prefix_cache",
            [QdrantCollectionRole.QdrantDocumentation] = "qdrant_documentation",

            // Identity
            [QdrantCollectionRole.Personalities] = "ouroboros_personalities",
            [QdrantCollectionRole.Persons] = "ouroboros_persons",
            [QdrantCollectionRole.SelfIndex] = "ouroboros_selfindex",
            [QdrantCollectionRole.FileHashes] = "ouroboros_filehashes",

            // Pipeline
            [QdrantCollectionRole.PipelineVectors] = "pipeline_vectors",

            // DAG
            [QdrantCollectionRole.DagNodes] = "ouroboros_dag_nodes",
            [QdrantCollectionRole.DagEdges] = "ouroboros_dag_edges",

            // Network
            [QdrantCollectionRole.NetworkSnapshots] = "network_state_snapshots",
            [QdrantCollectionRole.NetworkLearnings] = "network_learnings",

            // Learning
            [QdrantCollectionRole.DistinctionStates] = "distinction_states",

            // Episodic Memory
            [QdrantCollectionRole.EpisodicMemory] = "episodic_memory",

            // Admin
            [QdrantCollectionRole.CollectionMetadata] = "ouroboros_collection_metadata",
        };

    /// <summary>
    /// Reverse lookup: collection name → role.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, QdrantCollectionRole> DefaultReverseLookup =
        Defaults.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public QdrantCollectionRegistry(
        QdrantClient client,
        ILogger<QdrantCollectionRegistry>? logger = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger;

        // Seed with defaults
        foreach ((QdrantCollectionRole role, string? name) in Defaults)
        {
            _mappings[role] = name;
        }
    }

    /// <summary>
    /// Constructor with configuration overrides from appsettings.
    /// </summary>
    public QdrantCollectionRegistry(
        QdrantClient client,
        IOptions<QdrantCollectionOverrides> overrides,
        ILogger<QdrantCollectionRegistry>? logger = null)
        : this(client, logger)
    {
        // Apply any overrides from configuration
        if (overrides?.Value?.Overrides != null)
        {
            foreach ((QdrantCollectionRole role, string? name) in overrides.Value.Overrides)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    _mappings[role] = name;
                    _logger?.LogInformation(
                        "Collection override: {Role} → {Name}", role, name);
                }
            }
        }
    }

    /// <inheritdoc/>
    public string GetCollectionName(QdrantCollectionRole role)
    {
        if (_mappings.TryGetValue(role, out string? name))
            return name;

        throw new KeyNotFoundException(
            $"No collection mapping found for role '{role}'. " +
            $"Ensure the role is registered or call DiscoverAsync() first.");
    }

    /// <inheritdoc/>
    public bool TryGetCollectionName(QdrantCollectionRole role, out string? collectionName)
    {
        if (_mappings.TryGetValue(role, out string? name))
        {
            collectionName = name;
            return true;
        }

        collectionName = null;
        return false;
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<QdrantCollectionRole, string> GetAllMappings()
    {
        return new Dictionary<QdrantCollectionRole, string>(_mappings);
    }

    /// <inheritdoc/>
    public async Task DiscoverAsync(CancellationToken ct = default)
    {
        try
        {
            IReadOnlyList<string> collections = await _client.ListCollectionsAsync(ct);
            int discoveredCount = 0;

            foreach (string collectionName in collections)
            {
                // Check if this collection is already mapped via reverse lookup
                if (DefaultReverseLookup.TryGetValue(collectionName, out QdrantCollectionRole role))
                {
                    // Confirm the default mapping
                    _mappings[role] = collectionName;
                    discoveredCount++;
                }
                else
                {
                    // Try to infer role from naming convention
                    QdrantCollectionRole? inferredRole = InferRoleFromName(collectionName);
                    if (inferredRole != null && !_mappings.ContainsKey(inferredRole.Value))
                    {
                        _mappings[inferredRole.Value] = collectionName;
                        discoveredCount++;
                        _logger?.LogInformation(
                            "Discovered collection '{Name}' mapped to role {Role}",
                            collectionName, inferredRole.Value);
                    }
                    else
                    {
                        _logger?.LogDebug(
                            "Unmapped collection discovered: '{Name}'", collectionName);
                    }
                }
            }

            _logger?.LogInformation(
                "Qdrant collection discovery complete: {Discovered} collections confirmed, " +
                "{Total} total mappings",
                discoveredCount, _mappings.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Grpc.Core.RpcException ex)
        {
            _logger?.LogWarning(ex,
                "Qdrant gRPC error during collection discovery (status: {Status}). " +
                "Using configuration defaults.",
                ex.StatusCode);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex,
                "Qdrant collection discovery failed, using configuration defaults. " +
                "Ensure Qdrant is running at the configured endpoint.");
        }
    }

    /// <summary>
    /// Attempts to infer a collection role from a collection name using naming conventions.
    /// </summary>
    private static QdrantCollectionRole? InferRoleFromName(string collectionName)
    {
        string lower = collectionName.ToLowerInvariant();
        return lower switch
        {
            _ when lower.Contains("thought") && lower.Contains("relation") => QdrantCollectionRole.ThoughtRelations,
            _ when lower.Contains("thought") && lower.Contains("result") => QdrantCollectionRole.ThoughtResults,
            _ when lower.Contains("neuro") && lower.Contains("thought") => QdrantCollectionRole.NeuroThoughts,
            _ when lower.Contains("neuron") && lower.Contains("message") => QdrantCollectionRole.NeuronMessages,
            _ when lower.Contains("intention") => QdrantCollectionRole.Intentions,
            _ when lower.Contains("memor") && lower.Contains("episod") => QdrantCollectionRole.EpisodicMemory,
            _ when lower.Contains("memor") => QdrantCollectionRole.Memories,
            _ when lower.Contains("conversation") => QdrantCollectionRole.Conversations,
            _ when lower.Contains("skill") => QdrantCollectionRole.Skills,
            _ when lower.Contains("tool") && lower.Contains("pattern") => QdrantCollectionRole.ToolPatterns,
            _ when lower.Contains("personalit") => QdrantCollectionRole.Personalities,
            _ when lower.Contains("person") => QdrantCollectionRole.Persons,
            _ when lower.Contains("selfindex") || lower.Contains("self_index") => QdrantCollectionRole.SelfIndex,
            _ when lower.Contains("filehash") || lower.Contains("file_hash") => QdrantCollectionRole.FileHashes,
            _ when lower.Contains("dag") && lower.Contains("node") => QdrantCollectionRole.DagNodes,
            _ when lower.Contains("dag") && lower.Contains("edge") => QdrantCollectionRole.DagEdges,
            _ when lower.Contains("network") && lower.Contains("snapshot") => QdrantCollectionRole.NetworkSnapshots,
            _ when lower.Contains("network") && lower.Contains("learning") => QdrantCollectionRole.NetworkLearnings,
            _ when lower.Contains("distinction") => QdrantCollectionRole.DistinctionStates,
            _ when lower.Contains("pipeline") => QdrantCollectionRole.PipelineVectors,
            _ when lower.Contains("prefix") && lower.Contains("cache") => QdrantCollectionRole.PrefixCache,
            _ when lower.Contains("codebase") => QdrantCollectionRole.Codebase,
            _ when lower.Contains("metadata") => QdrantCollectionRole.CollectionMetadata,
            _ => null,
        };
    }
}

/// <summary>
/// Configuration overrides for collection name mappings, bound from appsettings.
/// </summary>
public sealed class QdrantCollectionOverrides
{
    /// <summary>
    /// Role-to-collection-name overrides from configuration.
    /// </summary>
    public Dictionary<QdrantCollectionRole, string> Overrides { get; set; } = new();
}
