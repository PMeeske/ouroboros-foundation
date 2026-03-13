using Ouroboros.Core.Learning;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Learning;

/// <summary>
/// Metadata about a stored distinction.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DistinctionMetadata(
    DistinctionId Id,
    string Circumstance,
    string StoragePath,
    int LearnedAtStage,
    double Fitness,
    bool IsDissolved,
    DateTime CreatedAt,
    DateTime? DissolvedAt);