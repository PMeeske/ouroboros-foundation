using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Event arguments for truth value changes in Laws of Form reasoning.
/// </summary>
public sealed class TruthValueEventArgs : EventArgs
{
    /// <summary>
    /// Gets the expression being evaluated.
    /// </summary>
    public required Atom Expression { get; init; }

    /// <summary>
    /// Gets the evaluated truth value as a Form.
    /// </summary>
    public Form TruthValue { get; init; }

    /// <summary>
    /// Gets the reasoning trace that led to this truth value.
    /// </summary>
    public ImmutableList<string> ReasoningTrace { get; init; } = ImmutableList<string>.Empty;

    /// <summary>
    /// Gets whether the truth value is certain (Mark or Void) or uncertain (Imaginary).
    /// </summary>
    public bool IsCertain => TruthValue.IsCertain();
}