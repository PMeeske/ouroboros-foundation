namespace Ouroboros.Core.Configuration;

/// <summary>
/// Typed roles for all known Qdrant collections.
/// Components request collections by role via <see cref="IQdrantCollectionRegistry"/>,
/// never by hardcoded string.
/// </summary>
public enum QdrantCollectionRole
{
    // ── Thought System ──────────────────────────────────
    /// <summary>Stores neuro-thought embeddings and semantic content.</summary>
    NeuroThoughts,

    /// <summary>Stores semantic relations between thoughts.</summary>
    ThoughtRelations,

    /// <summary>Stores results produced by thought processing.</summary>
    ThoughtResults,

    // ── Neural Memory ───────────────────────────────────
    /// <summary>Stores vectorised neuron-to-neuron messages.</summary>
    NeuronMessages,

    /// <summary>Stores intention embeddings for goal tracking.</summary>
    Intentions,

    /// <summary>Stores general memory embeddings.</summary>
    Memories,

    // ── Conversations ───────────────────────────────────
    /// <summary>Stores conversation turn embeddings.</summary>
    Conversations,

    // ── Skills & Tools ──────────────────────────────────
    /// <summary>Stores skill embeddings for retrieval.</summary>
    Skills,

    /// <summary>Stores learned tool-use patterns.</summary>
    ToolPatterns,

    /// <summary>Stores tool definition embeddings.</summary>
    Tools,

    // ── Knowledge ───────────────────────────────────────
    /// <summary>Stores core knowledge embeddings.</summary>
    Core,

    /// <summary>Stores the full core knowledge base embeddings.</summary>
    FullCore,

    /// <summary>Stores codebase embeddings for semantic code search.</summary>
    Codebase,

    /// <summary>Stores prefix-cache embeddings for prompt optimisation.</summary>
    PrefixCache,

    /// <summary>Stores Qdrant documentation embeddings for self-help retrieval.</summary>
    QdrantDocumentation,

    // ── Identity ────────────────────────────────────────
    /// <summary>Stores personality profile embeddings.</summary>
    Personalities,

    /// <summary>Stores person entity embeddings.</summary>
    Persons,

    /// <summary>Stores self-model index embeddings.</summary>
    SelfIndex,

    /// <summary>Stores file content hashes for change detection.</summary>
    FileHashes,

    // ── Pipeline ────────────────────────────────────────
    /// <summary>Stores intermediate pipeline vector outputs.</summary>
    PipelineVectors,

    // ── DAG ─────────────────────────────────────────────
    /// <summary>Stores DAG node embeddings.</summary>
    DagNodes,

    /// <summary>Stores DAG edge embeddings.</summary>
    DagEdges,

    // ── Network ─────────────────────────────────────────
    /// <summary>Stores point-in-time network state snapshots.</summary>
    NetworkSnapshots,

    /// <summary>Stores learned network adaptation embeddings.</summary>
    NetworkLearnings,

    // ── Learning ────────────────────────────────────────
    /// <summary>Stores serialised distinction-learning states.</summary>
    DistinctionStates,

    // ── Episodic Memory ─────────────────────────────────
    /// <summary>Stores episodic memory embeddings for experience recall.</summary>
    EpisodicMemory,

    // ── Embodiment ────────────────────────────────────────
    /// <summary>Stores embodied perception embeddings (vision, audio, sensor data).</summary>
    EmbodimentPerceptions,

    /// <summary>Stores embodiment state embeddings (pose, emotion, environment).</summary>
    EmbodimentStates,

    /// <summary>Stores affordance embeddings (available actions in current context).</summary>
    EmbodimentAffordances,

    // ── Avatar ────────────────────────────────────────────
    /// <summary>Stores cached comic panel embeddings for avatar generation.</summary>
    ComicPanels,

    // ── Admin ───────────────────────────────────────────
    /// <summary>Stores collection-level metadata embeddings.</summary>
    CollectionMetadata,
}
