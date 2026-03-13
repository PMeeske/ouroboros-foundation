using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Represents a temporal constraint for satisfiability checking.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record TemporalConstraint(
    string EventIdA,
    string EventIdB,
    TemporalRelation RequiredRelation,
    TimeSpan? MinGap = null,
    TimeSpan? MaxGap = null);
