namespace Ouroboros.Domain.Voice;

/// <summary>
/// Heartbeat event for presence detection and timeout handling.
/// </summary>
public sealed record HeartbeatEvent : InteractionEvent
{
    /// <summary>Gets the current state at heartbeat time.</summary>
    public AgentPresenceState CurrentState { get; init; }

    /// <summary>Gets the time since last user interaction.</summary>
    public TimeSpan TimeSinceLastInteraction { get; init; }
}