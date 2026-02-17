using Ouroboros.Domain.Vectors;

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Report of a memory health check operation.
/// </summary>
public sealed record MemoryHealthReport(
    int HealthyCollections,
    int UnhealthyCollections,
    IReadOnlyList<string> HealedCollections,
    IReadOnlyList<string> RemainingIssues,
    MemoryStatistics Statistics);