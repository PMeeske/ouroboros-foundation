using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Represents metadata about a Qdrant collection managed by Ouroboros.
/// </summary>
public sealed record CollectionInfo(
    string Name,
    ulong VectorSize,
    ulong PointsCount,
    Distance DistanceMetric,
    CollectionStatus Status,
    DateTime? CreatedAt = null,
    string? Purpose = null,
    IReadOnlyList<string>? LinkedCollections = null);