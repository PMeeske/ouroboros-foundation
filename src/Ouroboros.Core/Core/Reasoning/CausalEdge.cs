namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Represents a causal edge between two variables.
/// </summary>
/// <param name="Cause">The causing variable.</param>
/// <param name="Effect">The effect variable.</param>
/// <param name="Strength">The strength of the causal relationship.</param>
/// <param name="Type">The type of edge (direct, confounded, mediated, collider).</param>
public sealed record CausalEdge(
    string Cause,
    string Effect,
    double Strength,
    EdgeType Type);