namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Represents a homeostasis policy for maintaining affective equilibrium.
/// </summary>
public sealed record HomeostasisRule(
    Guid Id,
    string Name,
    string Description,
    SignalType TargetSignal,
    double LowerBound,
    double UpperBound,
    double TargetValue,
    HomeostasisAction Action,
    double Priority,
    bool IsActive,
    DateTime CreatedAt);