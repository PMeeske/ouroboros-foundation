namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Statistics for a Qdrant collection.
/// </summary>
public sealed record QdrantCollectionStats
{
    public required string Name { get; init; }
    public bool Exists { get; init; }
    public long PointCount { get; init; }
    public int VectorSize { get; init; }
}