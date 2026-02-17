namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Represents a planned intervention on the causal graph.
/// </summary>
/// <param name="TargetVariable">The variable to intervene on.</param>
/// <param name="NewValue">The value to set the variable to.</param>
/// <param name="ExpectedEffect">The expected effect size of the intervention.</param>
/// <param name="Confidence">Confidence level in the intervention effect.</param>
/// <param name="SideEffects">List of variables that may be affected as side effects.</param>
public sealed record Intervention(
    string TargetVariable,
    object NewValue,
    double ExpectedEffect,
    double Confidence,
    List<string> SideEffects);