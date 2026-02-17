namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Represents a probability distribution.
/// </summary>
/// <param name="Type">The type of distribution.</param>
/// <param name="Mean">The mean of the distribution.</param>
/// <param name="Variance">The variance of the distribution.</param>
/// <param name="Samples">Empirical samples from the distribution.</param>
/// <param name="Probabilities">Probability mass function for discrete distributions.</param>
public sealed record Distribution(
    DistributionType Type,
    double Mean,
    double Variance,
    List<double> Samples,
    Dictionary<object, double> Probabilities);