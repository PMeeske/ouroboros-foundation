namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Represents the result of a speech-to-text transcription.
/// </summary>
/// <param name="Text">The transcribed text.</param>
/// <param name="Language">The detected or specified language.</param>
/// <param name="Duration">Duration of the audio in seconds.</param>
/// <param name="Segments">Optional word/segment-level timestamps.</param>
public sealed record TranscriptionResult(
    string Text,
    string? Language = null,
    double? Duration = null,
    IReadOnlyList<TranscriptionSegment>? Segments = null);