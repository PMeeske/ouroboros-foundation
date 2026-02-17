namespace Ouroboros.Providers.SpeechToText;

/// <summary>
/// Represents a segment of transcribed audio with timing information.
/// </summary>
/// <param name="Text">The text of this segment.</param>
/// <param name="Start">Start time in seconds.</param>
/// <param name="End">End time in seconds.</param>
/// <param name="Confidence">Optional confidence score (0-1).</param>
public sealed record TranscriptionSegment(
    string Text,
    double Start,
    double End,
    double? Confidence = null);