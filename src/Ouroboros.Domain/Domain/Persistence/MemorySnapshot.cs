using Ouroboros.Domain.Vectors;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// A point-in-time snapshot of the memory system.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record MemorySnapshot(
    DateTime Timestamp,
    IReadOnlyList<CollectionInfo> Collections,
    IReadOnlyList<CollectionLink> Links,
    IReadOnlyDictionary<MemoryLayer, long> VectorsByLayer,
    MemoryStatistics Statistics);