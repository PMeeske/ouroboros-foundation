namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Represents a timeline constructed from a set of temporal events.
/// </summary>
public sealed record Timeline(
    IReadOnlyList<TemporalEvent> Events,
    IReadOnlyList<TemporalRelationEdge> Relations,
    DateTime EarliestTime,
    DateTime LatestTime,
    IReadOnlyDictionary<string, IReadOnlyList<TemporalEvent>> EventsByType);