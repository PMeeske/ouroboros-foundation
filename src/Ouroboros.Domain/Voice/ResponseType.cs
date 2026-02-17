namespace Ouroboros.Domain.Voice;

/// <summary>
/// Types of agent response content.
/// </summary>
public enum ResponseType
{
    /// <summary>Direct answer to user's question.</summary>
    Direct,

    /// <summary>Contextual narration or explanation.</summary>
    Narration,

    /// <summary>Describing a tool/action being taken.</summary>
    Action,

    /// <summary>Asking for clarification.</summary>
    Clarification,

    /// <summary>Inner thought spoken aloud (whisper style).</summary>
    InnerThought,
}