using LangChain.DocumentLoaders;
using Microsoft.Extensions.AI;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Extension methods for vector store operations.
/// Provides convenient wrappers for common retrieval patterns.
/// </summary>
public static class VectorStoreExtensions
{
    /// <summary>
    /// Retrieves similar documents from the vector store based on a text query.
    /// Automatically generates embeddings for the query text.
    /// </summary>
    /// <param name="store">The vector store to search</param>
    /// <param name="embeddingModel">Model to generate query embeddings</param>
    /// <param name="query">The text query to search for</param>
    /// <param name="amount">Number of similar documents to retrieve (default: 5)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of similar documents</returns>
    public static async Task<IReadOnlyCollection<Document>> GetSimilarDocuments(
        this IVectorStore store,
        IEmbeddingModel embeddingModel,
        string query,
        int amount = 5,
        CancellationToken cancellationToken = default)
    {
        if (store is null) throw new ArgumentNullException(nameof(store));
        if (embeddingModel is null) throw new ArgumentNullException(nameof(embeddingModel));
        if (query is null) query = string.Empty;

        float[] embedding = await embeddingModel.CreateEmbeddingsAsync(query, cancellationToken).ConfigureAwait(false);
        return await store.GetSimilarDocumentsAsync(embedding, amount, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves similar documents from the vector store based on a text query
    /// using a MEAI <see cref="IEmbeddingGenerator{String, Embedding}"/>.
    /// </summary>
    /// <param name="store">The vector store to search.</param>
    /// <param name="generator">MEAI embedding generator.</param>
    /// <param name="query">The text query to search for.</param>
    /// <param name="amount">Number of similar documents to retrieve (default: 5).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of similar documents.</returns>
    public static async Task<IReadOnlyCollection<Document>> GetSimilarDocuments(
        this IVectorStore store,
        IEmbeddingGenerator<string, Embedding<float>> generator,
        string query,
        int amount = 5,
        CancellationToken cancellationToken = default)
    {
        if (store is null) throw new ArgumentNullException(nameof(store));
        if (generator is null) throw new ArgumentNullException(nameof(generator));
        if (query is null) query = string.Empty;

        GeneratedEmbeddings<Embedding<float>> result = await generator
            .GenerateAsync([query], cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        float[] embedding = result.Count > 0 ? result[0].Vector.ToArray() : [];
        return await store.GetSimilarDocumentsAsync(embedding, amount, cancellationToken).ConfigureAwait(false);
    }
}
