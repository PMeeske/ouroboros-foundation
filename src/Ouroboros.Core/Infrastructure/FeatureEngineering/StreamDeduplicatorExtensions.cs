namespace Ouroboros.Infrastructure.FeatureEngineering;

/// <summary>
/// Extension methods for StreamDeduplicator to support fluent API.
/// </summary>
public static class StreamDeduplicatorExtensions
{
    /// <summary>
    /// Deduplicates a stream of vectors using the specified deduplicator.
    /// </summary>
    /// <param name="vectors">Input stream of vectors.</param>
    /// <param name="deduplicator">The deduplicator to use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of unique vectors.</returns>
    public static IAsyncEnumerable<float[]> Deduplicate(
        this IAsyncEnumerable<float[]> vectors,
        StreamDeduplicator deduplicator,
        CancellationToken cancellationToken = default)
    {
        if (vectors is null)
        {
            throw new ArgumentNullException(nameof(vectors));
        }

        if (deduplicator is null)
        {
            throw new ArgumentNullException(nameof(deduplicator));
        }

        return deduplicator.FilterStreamAsync(vectors, cancellationToken);
    }

    /// <summary>
    /// Deduplicates a collection of vectors using a new deduplicator instance.
    /// </summary>
    /// <param name="vectors">Input collection of vectors.</param>
    /// <param name="similarityThreshold">Similarity threshold for duplicate detection.</param>
    /// <param name="maxCacheSize">Maximum cache size.</param>
    /// <returns>A list of unique vectors.</returns>
    public static List<float[]> Deduplicate(
        this IEnumerable<float[]> vectors,
        float similarityThreshold = 0.95f,
        int maxCacheSize = 1000)
    {
        if (vectors is null)
        {
            throw new ArgumentNullException(nameof(vectors));
        }

        StreamDeduplicator deduplicator = new StreamDeduplicator(similarityThreshold, maxCacheSize);
        return deduplicator.FilterBatch(vectors);
    }
}