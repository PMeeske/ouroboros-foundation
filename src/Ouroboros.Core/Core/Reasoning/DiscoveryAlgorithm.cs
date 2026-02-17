namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Defines the algorithm used for causal structure discovery.
/// </summary>
public enum DiscoveryAlgorithm
{
    /// <summary>
    /// Peter-Clark algorithm for constraint-based causal discovery.
    /// </summary>
    PC,

    /// <summary>
    /// Fast Causal Inference algorithm.
    /// </summary>
    FCI,

    /// <summary>
    /// Greedy Equivalence Search algorithm.
    /// </summary>
    GES,

    /// <summary>
    /// Neural network based (NO TEARS) algorithm.
    /// </summary>
    NOTEARS,

    /// <summary>
    /// Continuous optimization DAGs with no curl constraint.
    /// </summary>
    DAGsNoCurl,
}