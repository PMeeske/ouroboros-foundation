namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Represents a causal relationship between two temporal events.
/// </summary>
public sealed record CausalRelation(
    TemporalEvent Cause,
    TemporalEvent Effect,
    double CausalStrength,
    string Mechanism,
    IReadOnlyList<string> ConfoundingFactors);