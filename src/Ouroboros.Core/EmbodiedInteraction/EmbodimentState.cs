namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Represents the agent's virtual embodiment state.
/// </summary>
public enum EmbodimentState
{
    /// <summary>Not active, no sensors running.</summary>
    Dormant,

    /// <summary>Sensors active, ready for interaction.</summary>
    Awake,

    /// <summary>Actively listening for audio input.</summary>
    Listening,

    /// <summary>Processing visual input.</summary>
    Observing,

    /// <summary>Generating speech output.</summary>
    Speaking,

    /// <summary>Processing/thinking.</summary>
    Processing,

    /// <summary>Multiple modalities active.</summary>
    FullyEngaged,
}