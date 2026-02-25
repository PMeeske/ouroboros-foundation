namespace Ouroboros.Domain.Voice;

/// <summary>
/// Styles for text output display.
/// </summary>
public enum OutputStyle
{
    /// <summary>Normal conversational output.</summary>
    Normal,

    /// <summary>Dimmed style for internal thoughts.</summary>
    Thinking,

    /// <summary>Emphasized/highlighted text.</summary>
    Emphasis,

    /// <summary>Softer/quieter display for whispers.</summary>
    Whisper,

    /// <summary>System messages.</summary>
    System,

    /// <summary>Error messages.</summary>
    Error,

    /// <summary>User input echo.</summary>
    UserInput,
}