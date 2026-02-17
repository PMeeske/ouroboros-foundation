namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Defines the type of probability distribution.
/// </summary>
public enum DistributionType
{
    /// <summary>
    /// Normal (Gaussian) distribution.
    /// </summary>
    Normal,

    /// <summary>
    /// Bernoulli distribution (binary outcomes).
    /// </summary>
    Bernoulli,

    /// <summary>
    /// Categorical distribution (discrete categories).
    /// </summary>
    Categorical,

    /// <summary>
    /// Empirical distribution from samples.
    /// </summary>
    Empirical,
}