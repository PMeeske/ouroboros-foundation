namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Represents a search result from Qdrant.
/// </summary>
internal sealed record QdrantSearchResult(string Id, double Score, Dictionary<string, object> Payload);