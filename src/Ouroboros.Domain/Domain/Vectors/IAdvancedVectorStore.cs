using LangChain.DocumentLoaders;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Extended vector store interface with advanced capabilities like filtering, counting, and batch operations.
/// </summary>
public interface IAdvancedVectorStore : IVectorStore
{
    /// <summary>
    /// Performs similarity search with metadata filtering.
    /// </summary>
    /// <param name="embedding">The query embedding.</param>
    /// <param name="filter">Metadata filter (key-value pairs to match).</param>
    /// <param name="amount">The number of results to return.</param>
    /// <param name="scoreThreshold">Minimum similarity score threshold (0-1).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of similar documents matching the filter.</returns>
    Task<IReadOnlyCollection<Document>> SearchWithFilterAsync(
        float[] embedding,
        IDictionary<string, object>? filter = null,
        int amount = 5,
        float? scoreThreshold = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the total number of vectors in the store.
    /// </summary>
    /// <param name="filter">Optional metadata filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of vectors.</returns>
    Task<ulong> CountAsync(IDictionary<string, object>? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scrolls through vectors with pagination support.
    /// </summary>
    /// <param name="limit">Maximum number of vectors to return.</param>
    /// <param name="offset">Offset for pagination (use null for first page).</param>
    /// <param name="filter">Optional metadata filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A scroll result with vectors and next offset.</returns>
    Task<ScrollResult> ScrollAsync(
        int limit = 10,
        string? offset = null,
        IDictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs batch similarity search with multiple query vectors.
    /// </summary>
    /// <param name="embeddings">The query embeddings.</param>
    /// <param name="amount">The number of results per query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of results for each query.</returns>
    Task<IReadOnlyList<IReadOnlyCollection<Document>>> BatchSearchAsync(
        IReadOnlyList<float[]> embeddings,
        int amount = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recommends similar vectors based on positive and negative examples.
    /// </summary>
    /// <param name="positiveIds">IDs of vectors to find similar to.</param>
    /// <param name="negativeIds">IDs of vectors to avoid similarity to.</param>
    /// <param name="amount">The number of results to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of recommended documents.</returns>
    Task<IReadOnlyCollection<Document>> RecommendAsync(
        IReadOnlyList<string> positiveIds,
        IReadOnlyList<string>? negativeIds = null,
        int amount = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes vectors by their IDs.
    /// </summary>
    /// <param name="ids">The IDs of vectors to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteByIdAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes vectors matching a metadata filter.
    /// </summary>
    /// <param name="filter">Metadata filter for vectors to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteByFilterAsync(IDictionary<string, object> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets collection/store information and statistics.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Store information including name, vector count, and configuration.</returns>
    Task<VectorStoreInfo> GetInfoAsync(CancellationToken cancellationToken = default);
}