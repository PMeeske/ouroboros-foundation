namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Represents a temporal relationship between two events using Allen Interval Algebra.
/// </summary>
public sealed record TemporalRelationEdge(
    TemporalEvent Event1,
    TemporalEvent Event2,
    TemporalRelationType RelationType,
    double Confidence);