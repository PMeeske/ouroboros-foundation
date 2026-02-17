namespace Ouroboros.Domain.Voice;

/// <summary>
/// Event args for state changes.
/// </summary>
public sealed class StateChangeEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StateChangeEventArgs"/> class.
    /// </summary>
    public StateChangeEventArgs(AgentPresenceState previousState, AgentPresenceState newState, string? reason)
    {
        PreviousState = previousState;
        NewState = newState;
        Reason = reason;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>Gets the previous state.</summary>
    public AgentPresenceState PreviousState { get; }

    /// <summary>Gets the new state.</summary>
    public AgentPresenceState NewState { get; }

    /// <summary>Gets the reason for the change.</summary>
    public string? Reason { get; }

    /// <summary>Gets the timestamp of the change.</summary>
    public DateTimeOffset Timestamp { get; }
}