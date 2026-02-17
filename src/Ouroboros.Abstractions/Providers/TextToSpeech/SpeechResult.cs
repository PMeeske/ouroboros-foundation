namespace Ouroboros.Providers.TextToSpeech;

/// <summary>
/// Represents the result of a text-to-speech synthesis.
/// </summary>
/// <param name="AudioData">The synthesized audio data.</param>
/// <param name="Format">The audio format (e.g., "mp3", "wav", "opus").</param>
/// <param name="Duration">Optional duration in seconds.</param>
public sealed record SpeechResult(
    byte[] AudioData,
    string Format,
    double? Duration = null);