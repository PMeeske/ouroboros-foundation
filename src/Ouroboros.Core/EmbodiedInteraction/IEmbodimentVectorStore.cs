// <copyright file="IEmbodimentVectorStore.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Vector-backed storage for embodiment state — perceptions, self-state snapshots,
/// and affordance maps. Provider-agnostic: implementations may use Qdrant, Pinecone,
/// in-memory, or any vector database.
/// </summary>
public interface IEmbodimentVectorStore : IAsyncDisposable
{
    /// <summary>
    /// Stores a fused perception as a vector for later episodic recall.
    /// The <paramref name="embedding"/> is produced by whichever embedding model
    /// the caller supplies — the store itself is model-agnostic.
    /// </summary>
    /// <param name="perception">The fused multimodal perception to store.</param>
    /// <param name="embedding">Pre-computed embedding vector.</param>
    /// <param name="ct">Cancellation token.</param>
    Task StorePerceptionAsync(FusedPerception perception, float[] embedding, CancellationToken ct = default);

    /// <summary>
    /// Recalls the most relevant past perceptions given a semantic query vector.
    /// </summary>
    /// <param name="queryEmbedding">Query vector (e.g. embedded from current context).</param>
    /// <param name="limit">Max results to return.</param>
    /// <param name="modalityFilter">Optional filter by dominant modality.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Recalled perceptions ordered by relevance, with similarity scores.</returns>
    Task<IReadOnlyList<RecalledPerception>> RecallPerceptionsAsync(
        float[] queryEmbedding,
        int limit = 5,
        SensorModality? modalityFilter = null,
        CancellationToken ct = default);

    /// <summary>
    /// Stores a snapshot of the virtual self's state as a vector for temporal recall.
    /// Enables "how was I feeling/what was I doing at time X?" queries.
    /// </summary>
    /// <param name="snapshot">The state snapshot to store.</param>
    /// <param name="embedding">Pre-computed embedding vector (from state description).</param>
    /// <param name="ct">Cancellation token.</param>
    Task StoreStateSnapshotAsync(EmbodimentStateSnapshot snapshot, float[] embedding, CancellationToken ct = default);

    /// <summary>
    /// Recalls past self-state snapshots most similar to the query.
    /// </summary>
    /// <param name="queryEmbedding">Query vector.</param>
    /// <param name="limit">Max results.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<RecalledStateSnapshot>> RecallStatesAsync(
        float[] queryEmbedding,
        int limit = 5,
        CancellationToken ct = default);

    /// <summary>
    /// Stores an affordance vector — what the agent can do in a given context.
    /// </summary>
    /// <param name="affordance">Affordance description to store.</param>
    /// <param name="embedding">Pre-computed embedding vector.</param>
    /// <param name="ct">Cancellation token.</param>
    Task StoreAffordanceAsync(AffordanceRecord affordance, float[] embedding, CancellationToken ct = default);

    /// <summary>
    /// Finds affordances most relevant to the current situational context.
    /// </summary>
    /// <param name="queryEmbedding">Query vector (embedded from situational description).</param>
    /// <param name="limit">Max results.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<ScoredAffordance>> FindAffordancesAsync(
        float[] queryEmbedding,
        int limit = 5,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the total number of stored embodiment vectors across all categories.
    /// </summary>
    Task<EmbodimentVectorCounts> GetCountsAsync(CancellationToken ct = default);

    /// <summary>
    /// Ensures underlying storage (collections/indices) exists. Idempotent.
    /// </summary>
    Task InitializeAsync(CancellationToken ct = default);
}

/// <summary>
/// A recalled perception with its similarity score.
/// </summary>
/// <param name="Perception">Stored perception metadata.</param>
/// <param name="Score">Cosine similarity score (0–1).</param>
public sealed record RecalledPerception(StoredPerceptionMeta Perception, float Score);

/// <summary>
/// Metadata stored alongside a perception vector (no raw frame data — vectors only).
/// </summary>
/// <param name="Id">Original perception ID.</param>
/// <param name="Timestamp">When the perception was captured.</param>
/// <param name="DominantModality">Audio, Visual, or Text.</param>
/// <param name="IntegratedUnderstanding">The fused textual understanding.</param>
/// <param name="Confidence">Overall confidence at capture time.</param>
public sealed record StoredPerceptionMeta(
    Guid Id,
    DateTime Timestamp,
    SensorModality DominantModality,
    string IntegratedUnderstanding,
    double Confidence);

/// <summary>
/// Point-in-time snapshot of the virtual self's embodiment state for vector storage.
/// </summary>
/// <param name="Id">Snapshot ID.</param>
/// <param name="Timestamp">When the snapshot was captured.</param>
/// <param name="State">Embodiment state at capture.</param>
/// <param name="Description">Human-readable state description (used for embedding).</param>
/// <param name="EnergyLevel">Energy level at capture (0–1).</param>
/// <param name="ActiveSensors">Sensors that were active.</param>
/// <param name="AttentionTarget">What the agent was attending to, if anything.</param>
public sealed record EmbodimentStateSnapshot(
    Guid Id,
    DateTime Timestamp,
    EmbodimentState State,
    string Description,
    double EnergyLevel,
    IReadOnlySet<SensorModality> ActiveSensors,
    string? AttentionTarget);

/// <summary>
/// A recalled state snapshot with similarity score.
/// </summary>
/// <param name="Snapshot">The stored snapshot.</param>
/// <param name="Score">Cosine similarity score (0–1).</param>
public sealed record RecalledStateSnapshot(EmbodimentStateSnapshot Snapshot, float Score);

/// <summary>
/// An affordance record for vector storage — what the agent can do in context.
/// </summary>
/// <param name="Id">Affordance ID.</param>
/// <param name="Description">Natural language affordance description (used for embedding).</param>
/// <param name="Type">Affordance type.</param>
/// <param name="Constraints">Any constraints on this affordance.</param>
/// <param name="CreatedAt">When this affordance was recorded.</param>
public sealed record AffordanceRecord(
    Guid Id,
    string Description,
    AffordanceType Type,
    string? Constraints,
    DateTime CreatedAt);

/// <summary>
/// A scored affordance from semantic search.
/// </summary>
/// <param name="Affordance">The affordance record.</param>
/// <param name="Score">Cosine similarity score (0–1).</param>
public sealed record ScoredAffordance(AffordanceRecord Affordance, float Score);

/// <summary>
/// Counts of stored vectors by category.
/// </summary>
/// <param name="Perceptions">Number of stored perception vectors.</param>
/// <param name="StateSnapshots">Number of stored state snapshot vectors.</param>
/// <param name="Affordances">Number of stored affordance vectors.</param>
public sealed record EmbodimentVectorCounts(long Perceptions, long StateSnapshots, long Affordances)
{
    /// <summary>Total vectors across all categories.</summary>
    public long Total => Perceptions + StateSnapshots + Affordances;
}
