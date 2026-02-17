namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Transcription result from STT model.
/// </summary>
/// <param name="Text">Transcribed text.</param>
/// <param name="Confidence">Confidence score 0-1.</param>
/// <param name="Language">Detected language.</param>
/// <param name="IsFinal">Is this a final result.</param>
/// <param name="StartTime">Start time offset.</param>
/// <param name="EndTime">End time offset.</param>
/// <param name="Words">Word-level timestamps if available.</param>
public sealed record TranscriptionResult(
    string Text,
    double Confidence,
    string? Language,
    bool IsFinal,
    TimeSpan StartTime,
    TimeSpan EndTime,
    IReadOnlyList<WordTiming>? Words);