// <copyright file="IEmbodimentMemoryFusion.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>


namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Interface for unified embodiment-memory fusion with hybrid search.
/// Combines dense vector search, sparse keyword matching, and payload filtering
/// for cross-modal memory retrieval across sensory modalities.
/// </summary>
public interface IEmbodimentMemoryFusion
{
    /// <summary>
    /// Stores an embodied memory with multimodal metadata.
    /// </summary>
    /// <param name="memory">The embodied memory to store.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The unique identifier of the stored memory, or an error message.</returns>
    Task<Result<Guid, string>> StoreMemoryAsync(
        EmbodiedMemory memory,
        CancellationToken ct = default);

    /// <summary>
    /// Hybrid search combining dense vectors + keyword + temporal decay + modality filter.
    /// </summary>
    /// <param name="query">The natural language query to search for.</param>
    /// <param name="options">Optional search parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ranked list of scored memories, or an error message.</returns>
    Task<Result<List<ScoredMemory>, string>> HybridSearchAsync(
        string query,
        MemorySearchOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Cross-modal retrieval: find memories from one modality that relate to another.
    /// </summary>
    /// <param name="query">The natural language query.</param>
    /// <param name="sourceModality">The modality context of the query (e.g. "visual").</param>
    /// <param name="targetModality">The target modality to retrieve (e.g. "auditory").</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ranked list of scored memories from the target modality, or an error message.</returns>
    Task<Result<List<ScoredMemory>, string>> CrossModalSearchAsync(
        string query,
        string sourceModality,
        string targetModality,
        int limit = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Consolidates similar memories into higher-level abstractions.
    /// </summary>
    /// <param name="similarityThreshold">Minimum cosine similarity to consider memories as related (0.0 to 1.0).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Consolidation statistics, or an error message.</returns>
    Task<Result<ConsolidationResult, string>> ConsolidateMemoriesAsync(
        double similarityThreshold = 0.85,
        CancellationToken ct = default);

    /// <summary>
    /// Gets memory importance distribution for adaptive forgetting.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Distribution statistics, or an error message.</returns>
    Task<Result<MemoryDistribution, string>> GetMemoryDistributionAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Performs importance-weighted decay on old memories.
    /// </summary>
    /// <param name="decayRate">The decay rate per day of age (0.0 to 1.0).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of memories updated, or an error message.</returns>
    Task<Result<int, string>> ApplyTemporalDecayAsync(
        double decayRate = 0.01,
        CancellationToken ct = default);
}

/// <summary>An embodied memory record with multimodal data.</summary>
/// <param name="Content">The textual content or description of the memory.</param>
/// <param name="Modality">The sensory modality (e.g. "visual", "auditory", "tactile", "proprioceptive").</param>
/// <param name="Importance">Importance weight (0.0 to 1.0).</param>
/// <param name="Metadata">Optional key-value metadata for additional context.</param>
/// <param name="Timestamp">Optional timestamp; defaults to UTC now if not provided.</param>
public sealed record EmbodiedMemory(
    string Content,
    string Modality,
    double Importance,
    Dictionary<string, string>? Metadata = null,
    DateTime? Timestamp = null);

/// <summary>A memory with its relevance score.</summary>
/// <param name="Id">The unique identifier of the memory.</param>
/// <param name="Content">The textual content of the memory.</param>
/// <param name="Modality">The sensory modality.</param>
/// <param name="Score">The relevance score from the search (0.0 to 1.0).</param>
/// <param name="Importance">The importance weight of the memory.</param>
/// <param name="Timestamp">When the memory was recorded.</param>
/// <param name="Metadata">Optional key-value metadata.</param>
public sealed record ScoredMemory(
    Guid Id,
    string Content,
    string Modality,
    double Score,
    double Importance,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = null);

/// <summary>Options for hybrid memory search.</summary>
/// <param name="Limit">Maximum number of results to return.</param>
/// <param name="ModalityFilter">Optional modality to filter results by.</param>
/// <param name="MinImportance">Optional minimum importance threshold.</param>
/// <param name="After">Optional lower bound on timestamp.</param>
/// <param name="Before">Optional upper bound on timestamp.</param>
/// <param name="TemporalDecayFactor">Factor controlling how much recency boosts score (0.0 = no boost).</param>
/// <param name="UseSparseKeywords">Whether to incorporate keyword matching in scoring.</param>
public sealed record MemorySearchOptions(
    int Limit = 10,
    string? ModalityFilter = null,
    double? MinImportance = null,
    DateTime? After = null,
    DateTime? Before = null,
    double TemporalDecayFactor = 0.1,
    bool UseSparseKeywords = true);

/// <summary>Result of memory consolidation.</summary>
/// <param name="MemoriesProcessed">Total memories evaluated during consolidation.</param>
/// <param name="ClustersMerged">Number of similarity clusters that were merged.</param>
/// <param name="NewAbstractions">Number of new abstraction points created.</param>
public sealed record ConsolidationResult(
    int MemoriesProcessed,
    int ClustersMerged,
    int NewAbstractions);

/// <summary>Distribution of memory importance levels.</summary>
/// <param name="TotalMemories">Total number of embodied memories stored.</param>
/// <param name="AverageImportance">Mean importance across all memories.</param>
/// <param name="HighImportance">Count of memories with importance >= 0.7.</param>
/// <param name="MediumImportance">Count of memories with importance >= 0.3 and less than 0.7.</param>
/// <param name="LowImportance">Count of memories with importance less than 0.3.</param>
/// <param name="ByModality">Memory count grouped by modality.</param>
public sealed record MemoryDistribution(
    int TotalMemories,
    double AverageImportance,
    int HighImportance,
    int MediumImportance,
    int LowImportance,
    Dictionary<string, int> ByModality);
