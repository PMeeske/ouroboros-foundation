namespace Ouroboros.Core.Processing;

/// <summary>
/// Represents an intermediate chunk result with its metadata.
/// </summary>
/// <typeparam name="TOutput">Type of the chunk output.</typeparam>
public sealed record ChunkResult<TOutput>(
    TOutput Output,
    ChunkMetadata Metadata,
    TimeSpan ProcessingTime,
    bool Success);