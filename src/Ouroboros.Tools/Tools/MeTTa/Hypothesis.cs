namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Represents a hypothesis with supporting evidence.
/// </summary>
/// <param name="Statement">The hypothesis statement.</param>
/// <param name="Plausibility">Plausibility score (0.0 to 1.0).</param>
/// <param name="SupportingEvidence">Supporting facts.</param>
public sealed record Hypothesis(
    string Statement,
    double Plausibility,
    List<Fact> SupportingEvidence);