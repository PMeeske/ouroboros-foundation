namespace Ouroboros.Domain.Voice;

/// <summary>
/// Control event for stream management (barge-in, cancel, etc.).
/// </summary>
public sealed record ControlEvent : InteractionEvent
{
    /// <summary>Gets the control action to perform.</summary>
    public required ControlAction Action { get; init; }

    /// <summary>Gets the reason for the control action.</summary>
    public string? Reason { get; init; }
}