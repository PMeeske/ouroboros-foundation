namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Represents a causal path from causes to an effect.
/// </summary>
/// <param name="Variables">The variables along the path.</param>
/// <param name="TotalEffect">The total causal effect along this path.</param>
/// <param name="IsDirect">Whether this is a direct path (no mediators).</param>
/// <param name="Edges">The edges comprising this path.</param>
public sealed record CausalPath(
    List<string> Variables,
    double TotalEffect,
    bool IsDirect,
    List<CausalEdge> Edges);