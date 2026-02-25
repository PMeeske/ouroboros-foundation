namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Represents a point to upsert to Qdrant.
/// </summary>
internal sealed record QdrantPoint
{
    public required string Id { get; init; }
    public required float[] Vector { get; init; }
    public required Dictionary<string, object> Payload { get; init; }
}