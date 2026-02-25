using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Event arguments for distinction-based reasoning events.
/// </summary>
public sealed class FormReasoningEventArgs : EventArgs
{
    /// <summary>
    /// Gets the reasoning operation type.
    /// </summary>
    public required string Operation { get; init; }

    /// <summary>
    /// Gets the form state involved.
    /// </summary>
    public Form FormState { get; init; }

    /// <summary>
    /// Gets the context identifier.
    /// </summary>
    public string? Context { get; init; }

    /// <summary>
    /// Gets any associated atoms.
    /// </summary>
    public IReadOnlyList<Atom> RelatedAtoms { get; init; } = Array.Empty<Atom>();

    /// <summary>
    /// Gets the reasoning trace.
    /// </summary>
    public IReadOnlyList<string> Trace { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}