namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Represents a structural equation in the causal model.
/// Defines how a variable's value is determined by its parents.
/// </summary>
/// <param name="Outcome">The outcome variable.</param>
/// <param name="Parents">The parent variables that influence the outcome.</param>
/// <param name="Function">The function mapping parent values to outcome value.</param>
/// <param name="NoiseVariance">The variance of the noise term.</param>
public sealed record StructuralEquation(
    string Outcome,
    List<string> Parents,
    Func<Dictionary<string, object>, object> Function,
    double NoiseVariance);