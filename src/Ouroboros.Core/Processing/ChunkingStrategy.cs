using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Processing;

/// <summary>
/// Defines the chunking strategy for processing large contexts.
/// </summary>
[ExcludeFromCodeCoverage]
public enum ChunkingStrategy
{
    /// <summary>
    /// Fixed chunk size - uses the specified maxChunkSize consistently.
    /// </summary>
    Fixed,

    /// <summary>
    /// Adaptive chunking - starts with maxChunkSize and adjusts based on processing success.
    /// Uses conditioned stimulus learning to optimize chunk size over time.
    /// </summary>
    Adaptive,
}
