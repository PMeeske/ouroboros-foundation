namespace Ouroboros.Domain.Voice;

/// <summary>
/// Event args for barge-in detection.
/// </summary>
public sealed class BargeInEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BargeInEventArgs"/> class.
    /// </summary>
    public BargeInEventArgs(AgentPresenceState interruptedState, string? userInput, BargeInType type)
    {
        InterruptedState = interruptedState;
        UserInput = userInput;
        Type = type;
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>Gets the state that was interrupted.</summary>
    public AgentPresenceState InterruptedState { get; }

    /// <summary>Gets the user input that caused the barge-in.</summary>
    public string? UserInput { get; }

    /// <summary>Gets the type of barge-in.</summary>
    public BargeInType Type { get; }

    /// <summary>Gets the timestamp.</summary>
    public DateTimeOffset Timestamp { get; }
}