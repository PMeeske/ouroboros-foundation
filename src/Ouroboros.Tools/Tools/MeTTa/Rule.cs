namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Represents a logical rule with premises and a conclusion.
/// </summary>
/// <param name="Name">Name of the rule.</param>
/// <param name="Premises">Premises that must be satisfied.</param>
/// <param name="Conclusion">Conclusion when premises are satisfied.</param>
/// <param name="Confidence">Confidence level (0.0 to 1.0).</param>
public sealed record Rule(
    string Name,
    List<Pattern> Premises,
    Pattern Conclusion,
    double Confidence = 1.0);