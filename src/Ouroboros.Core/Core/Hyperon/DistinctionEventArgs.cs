using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Event arguments for distinction events in the Laws of Form / MeTTa bridge.
/// </summary>
public sealed class DistinctionEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of distinction event.
    /// </summary>
    public DistinctionEventType EventType { get; init; }

    /// <summary>
    /// Gets the form state before the event.
    /// </summary>
    public Form? PreviousState { get; init; }

    /// <summary>
    /// Gets the form state after the event.
    /// </summary>
    public Form CurrentState { get; init; }

    /// <summary>
    /// Gets the atom that triggered this distinction (if any).
    /// </summary>
    public Atom? TriggerAtom { get; init; }

    /// <summary>
    /// Gets the name or identifier of the distinction context.
    /// </summary>
    public string? Context { get; init; }

    /// <summary>
    /// Gets the timestamp of the event.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}