using Ouroboros.Domain.Vectors;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Report of a memory health check operation.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record MemoryHealthReport(
    int HealthyCollections,
    int UnhealthyCollections,
    IReadOnlyList<string> HealedCollections,
    IReadOnlyList<string> RemainingIssues,
    MemoryStatistics Statistics);