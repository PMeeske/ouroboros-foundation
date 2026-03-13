namespace Ouroboros.Domain.Voice;

/// <summary>
/// Phases of agent thinking/reasoning.
/// </summary>
public enum ThinkingPhase
{
    /// <summary>Analyzing the user's input.</summary>
    Analyzing,

    /// <summary>Reasoning about the response.</summary>
    Reasoning,

    /// <summary>Planning actions or steps.</summary>
    Planning,

    /// <summary>Reflecting on the response.</summary>
    Reflecting,
}