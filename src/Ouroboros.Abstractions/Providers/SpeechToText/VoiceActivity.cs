namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Types of voice activity.
/// </summary>
public enum VoiceActivity
{
    /// <summary>Speech has started.</summary>
    SpeechStart,

    /// <summary>Speech has ended.</summary>
    SpeechEnd,

    /// <summary>Silence/no speech detected.</summary>
    Silence,

    /// <summary>Background noise detected (no speech).</summary>
    Noise,
}