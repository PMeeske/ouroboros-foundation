namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Defines the type of causal edge.
/// </summary>
public enum EdgeType
{
    /// <summary>
    /// Direct causal relationship.
    /// </summary>
    Direct,

    /// <summary>
    /// Relationship through a shared confounder.
    /// </summary>
    Confounded,

    /// <summary>
    /// Relationship mediated through another variable.
    /// </summary>
    Mediated,

    /// <summary>
    /// Common effect (collider).
    /// </summary>
    Collider,
}