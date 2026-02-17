namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Sensor modality types.
/// </summary>
public enum SensorModality
{
    /// <summary>Microphone/audio input.</summary>
    Audio,

    /// <summary>Camera/video input.</summary>
    Visual,

    /// <summary>Text/keyboard input.</summary>
    Text,

    /// <summary>Touch/haptic input.</summary>
    Haptic,

    /// <summary>Proprioceptive (internal state) sensing.</summary>
    Proprioceptive,
}