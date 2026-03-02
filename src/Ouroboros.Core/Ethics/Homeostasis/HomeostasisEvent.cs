namespace Ouroboros.Core.Ethics;

/// <summary>
/// A homeostasis event raised when the system's ethical balance shifts.
/// </summary>
public sealed record HomeostasisEvent
{
    /// <summary>Gets the type identifier for this homeostasis event (e.g. "TensionRegistered").</summary>
    public required string EventType { get; init; }

    /// <summary>Gets a human-readable description of the event.</summary>
    public required string Description { get; init; }

    /// <summary>Gets the homeostasis snapshot captured immediately before the event.</summary>
    public required HomeostasisSnapshot Before { get; init; }

    /// <summary>Gets the homeostasis snapshot captured immediately after the event.</summary>
    public required HomeostasisSnapshot After { get; init; }

    /// <summary>Gets the UTC timestamp when this event occurred.</summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
