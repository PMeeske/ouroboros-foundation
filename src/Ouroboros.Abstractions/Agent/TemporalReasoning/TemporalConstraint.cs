namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Represents a temporal constraint for satisfiability checking.
/// </summary>
public sealed record TemporalConstraint(
    string EventIdA,
    string EventIdB,
    TemporalRelation RequiredRelation,
    TimeSpan? MinGap = null,
    TimeSpan? MaxGap = null);