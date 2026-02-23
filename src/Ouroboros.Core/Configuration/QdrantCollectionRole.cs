namespace Ouroboros.Core.Configuration;

/// <summary>
/// Typed roles for all known Qdrant collections.
/// Components request collections by role via <see cref="IQdrantCollectionRegistry"/>,
/// never by hardcoded string.
/// </summary>
public enum QdrantCollectionRole
{
    // ── Thought System ──────────────────────────────────
    NeuroThoughts,
    ThoughtRelations,
    ThoughtResults,

    // ── Neural Memory ───────────────────────────────────
    NeuronMessages,
    Intentions,
    Memories,

    // ── Conversations ───────────────────────────────────
    Conversations,

    // ── Skills & Tools ──────────────────────────────────
    Skills,
    ToolPatterns,
    Tools,

    // ── Knowledge ───────────────────────────────────────
    Core,
    FullCore,
    Codebase,
    PrefixCache,
    QdrantDocumentation,

    // ── Identity ────────────────────────────────────────
    Personalities,
    Persons,
    SelfIndex,
    FileHashes,

    // ── Pipeline ────────────────────────────────────────
    PipelineVectors,

    // ── DAG ─────────────────────────────────────────────
    DagNodes,
    DagEdges,

    // ── Network ─────────────────────────────────────────
    NetworkSnapshots,
    NetworkLearnings,

    // ── Learning ────────────────────────────────────────
    DistinctionStates,

    // ── Episodic Memory ─────────────────────────────────
    EpisodicMemory,

    // ── Admin ───────────────────────────────────────────
    CollectionMetadata,
}
