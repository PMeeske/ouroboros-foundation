namespace Ouroboros.Domain.Voice;

/// <summary>
/// Types of barge-in events.
/// </summary>
public enum BargeInType
{
    /// <summary>User interrupted agent speech.</summary>
    SpeechInterrupt,

    /// <summary>User cancelled ongoing processing.</summary>
    ProcessingCancel,
}