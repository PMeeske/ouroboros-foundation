namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Represents a symbolic fact.
/// </summary>
/// <param name="Predicate">Predicate name.</param>
/// <param name="Arguments">List of arguments.</param>
/// <param name="Confidence">Confidence level (0.0 to 1.0).</param>
public sealed record Fact(
    string Predicate,
    List<string> Arguments,
    double Confidence = 1.0);