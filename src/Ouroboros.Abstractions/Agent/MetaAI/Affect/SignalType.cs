namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Types of valence signals.
/// </summary>
public enum SignalType
{
    /// <summary>Stress indicator from system load or failures</summary>
    Stress,

    /// <summary>Confidence from successful task completions</summary>
    Confidence,

    /// <summary>Curiosity from novelty detection</summary>
    Curiosity,

    /// <summary>General valence (positive/negative affect)</summary>
    Valence,

    /// <summary>Arousal level (energy/activation)</summary>
    Arousal
}