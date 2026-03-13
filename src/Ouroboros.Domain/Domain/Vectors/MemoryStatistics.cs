using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Statistics about Ouroboros's vector memory usage.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record MemoryStatistics(
    int TotalCollections,
    long TotalVectors,
    int HealthyCollections,
    int UnhealthyCollections,
    int CollectionLinks,
    IReadOnlyDictionary<ulong, int> DimensionDistribution);