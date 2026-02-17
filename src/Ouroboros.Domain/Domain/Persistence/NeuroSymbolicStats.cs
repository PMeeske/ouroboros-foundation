namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Statistics for the neuro-symbolic thought map.
/// </summary>
public sealed record NeuroSymbolicStats(
    int TotalThoughts,
    int TotalRelations,
    int TotalResults,
    Dictionary<string, int> ThoughtsByType,
    Dictionary<string, int> RelationsByType,
    Dictionary<string, int> ResultsByType,
    int CausalChainCount,
    double AverageChainLength,
    DateTime? OldestThought,
    DateTime? NewestThought);