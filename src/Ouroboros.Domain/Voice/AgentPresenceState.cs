namespace Ouroboros.Domain.Voice;

/// <summary>
/// Agent presence states for the embodiment state machine.
/// </summary>
public enum AgentPresenceState
{
    /// <summary>Waiting for input, ready to listen.</summary>
    Idle,

    /// <summary>Actively hearing/recording user speech.</summary>
    Listening,

    /// <summary>Processing input, generating response.</summary>
    Processing,

    /// <summary>Voice output is active.</summary>
    Speaking,

    /// <summary>User interrupted (barge-in detected).</summary>
    Interrupted,

    /// <summary>Paused/suspended state.</summary>
    Paused,
}