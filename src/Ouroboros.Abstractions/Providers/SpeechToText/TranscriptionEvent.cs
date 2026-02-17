namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Transcription event for streaming STT with partial/final distinction.
/// </summary>
/// <param name="Text">The transcribed text (may be partial).</param>
/// <param name="IsFinal">Whether this is a final result (stable) or interim (may change).</param>
/// <param name="Confidence">Confidence score (0.0-1.0).</param>
/// <param name="Offset">Time offset from start of audio.</param>
/// <param name="Duration">Duration of this transcription segment.</param>
/// <param name="Language">Detected language code.</param>
public sealed record TranscriptionEvent(
    string Text,
    bool IsFinal,
    double Confidence,
    TimeSpan Offset,
    TimeSpan Duration,
    string? Language = null);