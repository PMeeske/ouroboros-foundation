namespace Ouroboros.Core.Processing;

/// <summary>
/// Interface for processing large contexts by recursively chunking them into smaller pieces.
/// Implements map-reduce pattern for parallel chunk processing with adaptive chunking strategies.
/// </summary>
public interface IRecursiveChunkProcessor
{
    /// <summary>
    /// Processes a large context by chunking it and applying processing to each chunk.
    /// </summary>
    /// <typeparam name="TInput">Type of the input context.</typeparam>
    /// <typeparam name="TOutput">Type of the output result.</typeparam>
    /// <param name="largeContext">The large context to process.</param>
    /// <param name="maxChunkSize">Maximum chunk size in tokens (default: 512).</param>
    /// <param name="strategy">Chunking strategy to use (default: Adaptive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the processed output or error.</returns>
    Task<Result<TOutput>> ProcessLargeContextAsync<TInput, TOutput>(
        TInput largeContext,
        int maxChunkSize = 512,
        ChunkingStrategy strategy = ChunkingStrategy.Adaptive,
        CancellationToken cancellationToken = default);
}