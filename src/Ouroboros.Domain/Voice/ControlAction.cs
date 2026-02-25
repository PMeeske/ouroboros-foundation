namespace Ouroboros.Domain.Voice;

/// <summary>
/// Control actions for managing the interaction stream.
/// </summary>
public enum ControlAction
{
    /// <summary>Start listening for voice input.</summary>
    StartListening,

    /// <summary>Stop listening for voice input.</summary>
    StopListening,

    /// <summary>Interrupt current speech output (barge-in).</summary>
    InterruptSpeech,

    /// <summary>Cancel ongoing response generation.</summary>
    CancelGeneration,

    /// <summary>Reset to idle state.</summary>
    Reset,

    /// <summary>Pause all processing.</summary>
    Pause,

    /// <summary>Resume from paused state.</summary>
    Resume,
}