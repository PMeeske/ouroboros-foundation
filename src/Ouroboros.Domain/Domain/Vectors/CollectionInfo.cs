using Qdrant.Client.Grpc;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Represents metadata about a Qdrant collection managed by Ouroboros.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record CollectionInfo(
    string Name,
    ulong VectorSize,
    ulong PointsCount,
    Distance DistanceMetric,
    CollectionStatus Status,
    DateTime? CreatedAt = null,
    string? Purpose = null,
    IReadOnlyList<string>? LinkedCollections = null);