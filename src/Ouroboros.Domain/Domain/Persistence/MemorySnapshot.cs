using Ouroboros.Domain.Vectors;

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// A point-in-time snapshot of the memory system.
/// </summary>
public sealed record MemorySnapshot(
    DateTime Timestamp,
    IReadOnlyList<CollectionInfo> Collections,
    IReadOnlyList<CollectionLink> Links,
    IReadOnlyDictionary<MemoryLayer, long> VectorsByLayer,
    MemoryStatistics Statistics);