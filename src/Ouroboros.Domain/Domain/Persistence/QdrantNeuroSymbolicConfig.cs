namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Configuration for the neuro-symbolic thought store.
/// </summary>
/// <param name="Endpoint">Qdrant server endpoint.</param>
/// <param name="ThoughtsCollection">Collection for thought nodes.</param>
/// <param name="RelationsCollection">Collection for symbolic relations between thoughts.</param>
/// <param name="ResultsCollection">Collection for thought outcomes/results.</param>
/// <param name="VectorSize">Embedding dimension (default 768 for nomic-embed-text).</param>
public sealed record QdrantNeuroSymbolicConfig(
    string Endpoint = "http://localhost:6334",
    string ThoughtsCollection = "ouroboros_neuro_thoughts",
    string RelationsCollection = "ouroboros_thought_relations",
    string ResultsCollection = "ouroboros_thought_results",
    int VectorSize = 768);