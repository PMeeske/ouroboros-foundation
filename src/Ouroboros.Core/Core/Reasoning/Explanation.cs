namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Represents a causal explanation for an effect.
/// </summary>
/// <param name="Effect">The effect being explained.</param>
/// <param name="CausalPaths">The causal paths from causes to the effect.</param>
/// <param name="Attributions">Attribution scores for each potential cause.</param>
/// <param name="NarrativeExplanation">A human-readable narrative explanation.</param>
public sealed record Explanation(
    string Effect,
    List<CausalPath> CausalPaths,
    Dictionary<string, double> Attributions,
    string NarrativeExplanation);