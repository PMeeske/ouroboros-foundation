namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Parameters for querying temporal events.
/// </summary>
public sealed record TemporalQuery(
    DateTime? StartAfter = null,
    DateTime? StartBefore = null,
    DateTime? EndAfter = null,
    DateTime? EndBefore = null,
    string? EventType = null,
    int MaxResults = 100,
    DateTime? After = null,
    DateTime? Before = null,
    TimeSpan? Duration = null,
    Guid? RelatedEventId = null);