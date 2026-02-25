namespace Ouroboros.Domain.Voice;

/// <summary>
/// Voice input from user via microphone, after transcription.
/// </summary>
public sealed record VoiceInputEvent : InteractionEvent
{
    /// <summary>Gets the transcribed text from speech.</summary>
    public required string TranscribedText { get; init; }

    /// <summary>Gets the confidence score (0.0-1.0) of transcription.</summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>Gets the duration of the speech segment.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Gets the detected language code (e.g., "en-US").</summary>
    public string? DetectedLanguage { get; init; }

    /// <summary>Gets whether this is a partial/interim result.</summary>
    public bool IsInterim { get; init; }
}