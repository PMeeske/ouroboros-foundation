using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Represents a causal relationship between two temporal events.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record CausalRelation(
    TemporalEvent Cause,
    TemporalEvent Effect,
    double CausalStrength,
    string Mechanism,
    IReadOnlyList<string> ConfoundingFactors);
