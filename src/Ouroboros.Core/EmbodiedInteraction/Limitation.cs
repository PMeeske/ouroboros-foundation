namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Limitation or constraint of the virtual self.
/// </summary>
/// <param name="Type">Type of limitation.</param>
/// <param name="Description">Human-readable description.</param>
/// <param name="Severity">Severity level (0.0-1.0).</param>
public sealed record Limitation(
    LimitationType Type,
    string Description,
    double Severity = 0.5);