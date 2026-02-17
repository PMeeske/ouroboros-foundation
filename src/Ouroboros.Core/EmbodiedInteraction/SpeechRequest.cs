namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Speech synthesis request.
/// </summary>
/// <param name="Text">Text to synthesize.</param>
/// <param name="Priority">Priority (higher = more urgent).</param>
/// <param name="Emotion">Optional emotion to convey.</param>
/// <param name="Interruptible">Can this be interrupted by barge-in.</param>
public sealed record SpeechRequest(
    string Text,
    int Priority = 0,
    string? Emotion = null,
    bool Interruptible = true);