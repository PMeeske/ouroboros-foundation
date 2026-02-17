namespace Ouroboros.Core.Configuration;

/// <summary>
/// Configuration for vector store operations.
/// </summary>
public class VectorStoreConfiguration
{
    /// <summary>
    /// Gets or sets the type of vector store to use ("InMemory", "Qdrant", "Pinecone", etc.).
    /// </summary>
    public string Type { get; set; } = "InMemory";

    /// <summary>
    /// Gets or sets connection string for external vector stores.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets batch size for vector operations.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets default collection/index name.
    /// </summary>
    public string DefaultCollection { get; set; } = "pipeline_vectors";
}