
namespace Ouroboros.Domain;

/// <summary>
/// Simple embedding abstraction used across the pipeline.
/// Provides a wrapper around various embedding providers with a consistent interface.
/// </summary>
[Obsolete("Use IOuroborosEmbeddingGenerator (which extends IEmbeddingGenerator from Microsoft.Extensions.AI) instead. Will be removed in v3.")]
public interface IEmbeddingModel
{
    /// <summary>Generates an embedding vector for the given text input.</summary>
    /// <param name="input">The text to embed.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A float array representing the embedding vector.</returns>
    Task<float[]> CreateEmbeddingsAsync(string input, CancellationToken ct = default);
}
