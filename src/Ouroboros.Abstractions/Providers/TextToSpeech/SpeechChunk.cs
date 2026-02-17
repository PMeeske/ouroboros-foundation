namespace Ouroboros.Providers.TextToSpeech;

/// <summary>
/// A chunk of synthesized audio with metadata for streaming TTS.
/// </summary>
/// <param name="AudioData">The raw audio data.</param>
/// <param name="Format">The audio format (e.g., "pcm16", "mp3", "wav").</param>
/// <param name="DurationSeconds">Duration of this chunk in seconds.</param>
/// <param name="Text">The text this chunk represents (for debugging/display).</param>
/// <param name="IsSentenceEnd">Indicates a natural pause point (sentence boundary).</param>
/// <param name="IsComplete">True if this is the final chunk.</param>
public sealed record SpeechChunk(
    byte[] AudioData,
    string Format,
    double DurationSeconds,
    string? Text = null,
    bool IsSentenceEnd = false,
    bool IsComplete = false);