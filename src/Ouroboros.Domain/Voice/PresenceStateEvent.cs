namespace Ouroboros.Domain.Voice;

/// <summary>
/// Agent presence state change event.
/// </summary>
public sealed record PresenceStateEvent : InteractionEvent
{
    /// <summary>Gets the new presence state.</summary>
    public required AgentPresenceState State { get; init; }

    /// <summary>Gets the previous presence state.</summary>
    public AgentPresenceState? PreviousState { get; init; }

    /// <summary>Gets the reason for the state change.</summary>
    public string? Reason { get; init; }
}