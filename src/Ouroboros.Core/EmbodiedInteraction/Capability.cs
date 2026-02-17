namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Capability that the virtual self possesses.
/// </summary>
public enum Capability
{
    /// <summary>Can perceive audio/speech.</summary>
    Hearing,

    /// <summary>Can perceive visual information.</summary>
    Seeing,

    /// <summary>Can produce speech.</summary>
    Speaking,

    /// <summary>Can read/process text.</summary>
    Reading,

    /// <summary>Can produce text.</summary>
    Writing,

    /// <summary>Can reason about problems.</summary>
    Reasoning,

    /// <summary>Can remember past interactions.</summary>
    Remembering,

    /// <summary>Can learn from experience.</summary>
    Learning,

    /// <summary>Can reflect on own processes.</summary>
    Reflecting,

    /// <summary>Can plan future actions.</summary>
    Planning,

    /// <summary>Can use external tools.</summary>
    ToolUse,

    /// <summary>Can perceive emotional cues.</summary>
    EmotionPerception,

    /// <summary>Can express emotional states.</summary>
    EmotionExpression
}